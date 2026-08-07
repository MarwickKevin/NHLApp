using NHLApp.Domain.Entities;
using NHLApp.Domain.Interfaces;
using System.Text.Json;
using NHLApp.Application.DTOs;
using Microsoft.Extensions.Logging;
using NHLApp.Application.Extensions;
using System.Net;
using NHLApp.Application.Contexts;

namespace NHLApp.Application.Services
{
    // TODO: Add retry logic for API calls to handle transient failures and improve reliability
    // TODO: Make error handling consistent across all methods, including logging and exception throwing.
    // TODO: Use hash comparison for JSON content to determine if an update is necessary, instead of relying solely on timestamps
    // TODO: Improve commenting and documentation for each method, including parameter descriptions and return values.
    // TODO: Add counters of success (x/y) and show in console

    public class ImportService
    {
        private readonly INHLApiClient _nhlClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ImportService> _logger;

        // Constants for API throttling to avoid hitting the NHL API too quickly
        private const int ApiThrottlingDelay = 100;

        public ImportService(INHLApiClient nhlClient, IUnitOfWork unitOfWork, ILogger<ImportService> logger)
        {
            _nhlClient = nhlClient;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Import Methods

        /// <summary>
        /// Imports the latest seasons from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportSeasonsAsync(WorkerContext context)
        {
            // Check if the seasons have already been imported within the last 24 hours
            await ProcessImportAsync(
                context,
                endpoint: "season",
                entityId: "all",
                fetchApiAsync: () => _nhlClient.GetSeasonsAsync());
        }

        /// <summary>
        /// Imports the latest teams from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportTeamsAsync(WorkerContext context)
        {
            await ProcessImportAsync(
                context,
                endpoint: "team",
                entityId: "all",
                fetchApiAsync: () => _nhlClient.GetTeamsAsync(),
                // Extract team tricodes from the JSON response for metadata storage
                metadataSelector: json => ExtractTricodeMetadata(json));

        }

        /// <summary>
        /// Imports the roster seasons for each team from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportRosterSeasonsAsync(WorkerContext context)
        {
            var triCodes = GetTeamTriCodesFromMetadata();
            if (!triCodes.Any())
            {
                context.TotalImportErrors++;
                _logger.LogErrorWithColor(
                    "Cannot process because raw team data has not been imported yet. Total import errors: {TotalErrors}",
                    ConsoleColor.Red,
                    context.TotalImportErrors);
                return;
            }

            await ProcessCollectionImportAsync(
                context,
                items: triCodes,
                endpoint: "roster-seasons",
                entityIdSelector: triCode => triCode,
                fetchApiAsync: triCode => _nhlClient.GetTeamRosterSeasonsAsync(triCode),
                // Extract season IDs from the roster seasons JSON response for metadata storage
                metadataSelector: json => ExtractSeasonIdsMetadata(json));
        }

        /// <summary>
        /// Imports the rosters for each team and season from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportRostersAsync(WorkerContext context)
        {
            // Extract team tricodes from the raw team data
            var triCodes = GetTeamTriCodesFromMetadata();
            if (!triCodes.Any())
            {
                context.TotalImportErrors++;
                _logger.LogErrorWithColor(
                    "Cannot process rosters because raw team data has not been imported yet. Total import errors: {TotalErrors}",
                    ConsoleColor.Red,
                    context.TotalImportErrors);
                return;
            }

            // Check if the roster-seasons data has been imported for all teams
            var allRosterSeasonsExist = _unitOfWork.RawApiResponses.Any(r => r.Endpoint == "roster-seasons");
            if (!allRosterSeasonsExist)
            {
                context.TotalImportErrors++;
                _logger.LogErrorWithColor("Cannot process rosters because roster-seasons data has not been imported yet.", ConsoleColor.Red, context.TotalImportErrors);
                return;
            }

            // For each team, retrieve the roster seasons and then import the roster for each season
            foreach (var triCode in triCodes)
            {
                var seasonIds = GetSeasonIdsFromMetadata(triCode);

                // If the roster seasons data is missing or cannot be deserialized, skip to the next team
                if (!seasonIds.Any())
                {
                    context.TotalImportErrors++;
                    _logger.LogErrorWithColor("Failed to find roster seasons metadata for team {TriCode}. Total errors: {TotalErrors}", ConsoleColor.Red, triCode, context.TotalImportErrors);
                    continue;
                }

                await ProcessCollectionImportAsync(
                    context,
                    items: seasonIds,
                    endpoint: "roster",
                    entityIdSelector: seasonId => $"{triCode}-{seasonId}",
                    fetchApiAsync: seasonId => _nhlClient.GetTeamRosterAsync(triCode, seasonId),
                    // Extract player IDs from the roster JSON response for metadata storage
                    metadataSelector: json => ExtractPlayerIdsMetadata(json));

            }
        }

        /// <summary>
        /// Imports the player landing pages for each collected player ID from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportPlayerLandingsAsync(WorkerContext context)
        {
            var playerIds = GetAllPlayerIdsFromMetadata();

            if (!playerIds.Any())
            {
                _logger.LogInformationWithColor("No player IDs found in context to import player landings.", ConsoleColor.Yellow);
                return;
            }

            _logger.LogInformationWithColor("Starting import of player landings for {Count} unique players...", ConsoleColor.Cyan, playerIds.Count);

            await ProcessCollectionImportAsync(
                context,
                items: playerIds,
                endpoint: "player-landing",
                entityIdSelector: playerId => playerId.ToString(),
                fetchApiAsync: playerId => _nhlClient.GetPlayerLandingAsync(playerId));
        }
        #endregion

        #region Import Processing Engines

        /// <summary>
        /// Processes a single item, fetching data from the NHL API and storing it in the database if it hasn't been fetched recently.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="entityId"></param>
        /// <param name="fetchApiAsync"></param>
        /// <param name="metadataSelector"></param>
        /// <returns></returns>
        private async Task ProcessImportAsync(WorkerContext context, string endpoint, string entityId, Func<Task<string>> fetchApiAsync, Func<string, string?>? metadataSelector = null)
        {
            var key = $"{endpoint}-{entityId}";
            var existing = _unitOfWork.RawApiResponses
                .FirstOrDefault(r => r.EntityId == key);

            if (IsFresh(existing?.FetchedAt))
            {
                _logger.LogInformationWithColor("{Key} is fresh, skipping import.", ConsoleColor.DarkGreen, key);
                return;
            }

            try
            {
                var json = await fetchApiAsync();
                string? metadata = metadataSelector?.Invoke(json);

                await _unitOfWork.SaveOrUpdateRawResponseAsync(endpoint, key, json, metadata);

                context.TotalApiCalls++;
                _logger.LogInformationWithColor("TOTAL API CALLS SINCE APP STARTED: {TotalApiCalls}", ConsoleColor.Magenta, context.TotalApiCalls);

            }
            catch (Exception ex)
            {
                context.TotalImportErrors++;
                _logger.LogErrorWithColor(ex, "Failed to import endpoint {Endpoint} for entity {EntityId}. Total import errors: {TotalErrors}",
                    ConsoleColor.Red,
                    endpoint,
                    entityId,
                    context.TotalImportErrors);
            }
        }

        /// <summary>
        /// Processes a collection of items, fetching data from the NHL API for each item and storing it in the database if it hasn't been fetched recently.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="endpoint"></param>
        /// <param name="entityIdSelector"></param>
        /// <param name="fetchApiAsync"></param>
        /// <returns></returns>
        private async Task ProcessCollectionImportAsync<T>(WorkerContext context, IEnumerable<T> items, string endpoint, Func<T, string> entityIdSelector, Func<T, Task<string>> fetchApiAsync, Func<string, string?>? metadataSelector = null)
        {
            var existingRecords = _unitOfWork.RawApiResponses
                .Where(r => r.Endpoint == endpoint)
                .Select(r => new { r.EntityId, r.FetchedAt })
                .ToDictionary(r => r.EntityId, r => r.FetchedAt);

            foreach (var item in items)
            {
                var entityId = entityIdSelector(item);
                var key = $"{endpoint}-{entityId}";

                existingRecords.TryGetValue(key, out var fetchedAt);

                if (IsFresh(fetchedAt))
                {
                    _logger.LogInformationWithColor("{Key} is fresh, skipping import.", ConsoleColor.DarkGreen, key);
                    continue;
                }


                try
                {
                    var json = await fetchApiAsync(item);
                    string? metadata = metadataSelector?.Invoke(json);

                    await _unitOfWork.SaveOrUpdateRawResponseAsync(endpoint, key, json, metadata);

                    await Task.Delay(ApiThrottlingDelay);

                    context.TotalApiCalls++;
                    _logger.LogInformationWithColor("TOTAL API CALLS SINCE APP STARTED: {TotalApiCalls}", ConsoleColor.Magenta, context.TotalApiCalls);
                }
                catch (Exception ex)
                {
                    context.TotalImportErrors++;
                    _logger.LogErrorWithColor(ex,
                        "Failed to import endpoint {Endpoint} for entity {EntityId}. Total import errors: {TotalErrors}",
                        ConsoleColor.Red,
                        endpoint,
                        entityId,
                        context.TotalImportErrors);
                }
            }
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if the data fetched at the specified time is still fresh (i.e., fetched within the last 24 hours).
        /// </summary>
        private bool IsFresh(DateTime? fetchedAt)
        {
            return fetchedAt.HasValue && fetchedAt.Value > DateTime.UtcNow.AddDays(-1);
        }

        #endregion

        #region Metadata Extract and Get Methods
        

        /// <summary>
        /// Extracts the team tricodes from the raw JSON response of the team endpoint and returns them as a serialized JSON string for metadata storage.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private string? ExtractTricodeMetadata(string json)
        {
            if (json.TryDeserializeSafe<NhlTeamRootDTO>(out var teamRoot, out _) && teamRoot?.Data != null)
            {
                var triCodes = teamRoot.Data
                    .Where(t => t.TriCode != "TBD" && t.TriCode != "NHL")
                    .Select(t => t.TriCode)
                    .Distinct()
                    .ToList();

                return JsonSerializer.Serialize(new { TriCodes = triCodes });
            }
            return null;
        }
        /// <summary>
        /// Retrieves a list of valid team tricodes from the team endpoint metadata.
        /// </summary>
        private List<string> GetTeamTriCodesFromMetadata()
        {
            var teamRaw = _unitOfWork.RawApiResponses.FirstOrDefault(r => r.Endpoint == "team");
            if (teamRaw == null || string.IsNullOrWhiteSpace(teamRaw.Metadata)) return new List<string>();

            try
            {
                using var doc = JsonDocument.Parse(teamRaw.Metadata);
                if (doc.RootElement.TryGetProperty("TriCodes", out var triCodesElement) && triCodesElement.ValueKind == JsonValueKind.Array)
                {
                    return triCodesElement.EnumerateArray()
                        .Select(e => e.GetString()!)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                }
            }
            catch { _logger.LogErrorWithColor("Error parsing Tricodes for metadata", ConsoleColor.Red);}

            return new List<string>();
        }


        /// <summary>
        /// Extracts the season Ids from the roster-seasons JSON response and returns them as a serialized JSON string for metadata storage.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private string? ExtractSeasonIdsMetadata(string json)
        {
            if (json.TryDeserializeSafe<List<int>>(out var seasons, out _) && seasons != null)
            {
                return JsonSerializer.Serialize(new { SeasonIds = seasons });
            }
            return null;
        }
        /// <summary>
        /// Retrieves the season Ids for a specific team from the roster-seasons metadata.
        /// </summary>
        private List<int> GetSeasonIdsFromMetadata(string triCode)
        {
            var key = $"roster-seasons-{triCode}";
            var record = _unitOfWork.RawApiResponses
                .FirstOrDefault(r => r.Endpoint == "roster-seasons" && r.EntityId == key);

            if (record == null || string.IsNullOrWhiteSpace(record.Metadata)) return new List<int>();

            try
            {
                using var doc = JsonDocument.Parse(record.Metadata);
                if (doc.RootElement.TryGetProperty("SeasonIds", out var idsElement) && idsElement.ValueKind == JsonValueKind.Array)
                {
                    return idsElement.EnumerateArray()
                        .Where(e => e.TryGetInt32(out _))
                        .Select(e => e.GetInt32())
                        .ToList();
                }
            }
            catch {_logger.LogErrorWithColor("Error parsing SeasonIds for metadata", ConsoleColor.Red);}

            return new List<int>();
        }


        /// <summary>
        /// Extracts all unique player Ids from the roster JSON response and returns them as a serialized JSON string for metadata storage.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private string? ExtractPlayerIdsMetadata(string json)
        {
            if (json.TryDeserializeSafe<NhlRosterRootDTO>(out var rosterRoot, out _) && rosterRoot != null)
            {
                var playerIds = new HashSet<int>();
                if (rosterRoot.Forwards != null) foreach (var p in rosterRoot.Forwards) playerIds.Add(p.Id);
                if (rosterRoot.Defensemen != null) foreach (var p in rosterRoot.Defensemen) playerIds.Add(p.Id);
                if (rosterRoot.Goalies != null) foreach (var p in rosterRoot.Goalies) playerIds.Add(p.Id);

                return JsonSerializer.Serialize(new { PlayerIds = playerIds });
            }
            return null;
        }
        /// <summary>
        /// Retrieves all unique player Ids from the roster metadata.
        /// </summary>
        private List<int> GetAllPlayerIdsFromMetadata()
        {
            var playerIds = new List<int>();

            var records = _unitOfWork.RawApiResponses
                .Where(r => r.Endpoint == "roster" && !string.IsNullOrWhiteSpace(r.Metadata))
                .ToList();

            foreach (var record in records)
            {
                try
                {
                    using var doc = JsonDocument.Parse(record.Metadata!);
                    if (doc.RootElement.TryGetProperty("PlayerIds", out var idsElement) && idsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var e in idsElement.EnumerateArray())
                        {
                            if (e.TryGetInt32(out var id))
                            {
                                playerIds.Add(id);
                            }
                        }
                    }
                }
                catch { _logger.LogErrorWithColor("Error parsing PlayerId for metadata", ConsoleColor.Red); }
            }

            return playerIds.Distinct().ToList();
        }


        #endregion
    }
}