using NHLApp.Domain.Entities;
using NHLApp.Domain.Interfaces;
using NHLApp.Infrastructure.Data;
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
        private readonly NHLAppDbContext _db;
        private readonly ILogger<ImportService> _logger;
        private readonly RawDataStore _rawDataStore;

        // Constants for API throttling to avoid hitting the NHL API too quickly
        private const int ApiThrottlingDelay = 150;

        public ImportService(INHLApiClient nhlClient, NHLAppDbContext db, ILogger<ImportService> logger, RawDataStore rawDataStore)
        {
            _nhlClient = nhlClient;
            _db = db;
            _logger = logger;
            _rawDataStore = rawDataStore;
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
                fetchApiAsync: () => _nhlClient.GetTeamsAsync());
        }

        /// <summary>
        /// Imports the roster seasons for each team from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportRosterSeasonsAsync(WorkerContext context)
        {
            var triCodes = GetValidTeamTriCodes(context);
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
                fetchApiAsync: triCode => _nhlClient.GetTeamRosterSeasonsAsync(triCode));
        }

        /// <summary>
        /// Imports the rosters for each team and season from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportRostersAsync(WorkerContext context)
        {
            // Extract team tricodes from the raw team data
            var triCodes = GetValidTeamTriCodes(context);
            if (!triCodes.Any())
            {
                context.TotalImportErrors++;
                _logger.LogErrorWithColor(
                    "Cannot process rosters because raw team data has not been imported yet. Total import errors: {TotalErrors}",
                    ConsoleColor.Red,
                    context.TotalImportErrors);
                return;
            }
            
            var allRosterSeasons = _db.RawApiResponses
                .Where(r => r.Endpoint == "roster-seasons")
                .ToDictionary(r => r.EntityId, r => r.ResponseJson);

            // For each team, retrieve the roster seasons and then import the roster for each season
            foreach (var triCode in triCodes)
            {
                var key = $"roster-seasons-{triCode}";

                // If the roster seasons data is missing or cannot be deserialized, skip to the next team
                if (!allRosterSeasons.TryGetValue(key, out var responseJson) ||
                    !responseJson.TryDeserializeSafe<List<int>>(out var seasonIds, out _) ||
                    seasonIds == null)
                {
                    context.TotalImportErrors++;
                    _logger.LogErrorWithColor(
                        "Failed to import roster seasons for team {TriCode}. Total import errors: {TotalErrors}", 
                        ConsoleColor.Red, 
                        triCode, 
                        context.TotalImportErrors);
                    continue;
                }

                // Process each season for the team, fetching the roster data and storing it in the database
                await ProcessCollectionImportAsync(
                    context,
                    items: seasonIds,
                    endpoint: "roster",
                    entityIdSelector: seasonId => $"{triCode}-{seasonId}",
                    fetchApiAsync: seasonId => _nhlClient.GetTeamRosterAsync(triCode, seasonId));
            }
        }

        public async Task ImportPlayerLandings(WorkerContext context)
        {
            // Import playerIds from context
            // Go through each playerId and fetch the landing page data from the NHL API
        }
        #endregion

        #region Import Processing Engines

        /// <summary>
        /// Processes a single item, fetching data from the NHL API and storing it in the database if it hasn't been fetched recently.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="entityId"></param>
        /// <param name="fetchApiAsync"></param>
        /// <returns></returns>
        private async Task ProcessImportAsync(WorkerContext context, string endpoint, string entityId, Func<Task<string>> fetchApiAsync)
        {
            var key = $"{endpoint}-{entityId}";
            var existing = _db.RawApiResponses
                .FirstOrDefault(r => r.EntityId == key);

            if (IsFresh(existing?.FetchedAt))
            {
                _logger.LogInformationWithColor("{Key} is fresh, skipping import.", ConsoleColor.DarkGreen, key);
                return;
            }                

            try
            {
                var json = await fetchApiAsync();
                await _rawDataStore.SaveOrUpdateAsync(endpoint, key, json);

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
        private async Task ProcessCollectionImportAsync<T>(WorkerContext context, IEnumerable<T> items, string endpoint, Func<T, string> entityIdSelector, Func<T, Task<string>> fetchApiAsync)
        {
            var existingRecords = _db.RawApiResponses
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

                    await _rawDataStore.SaveOrUpdateAsync(endpoint, key, json);

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
        /// <param name="fetchedAt"></param>
        /// <returns></returns>
        private bool IsFresh(DateTime? fetchedAt)
        {
            return fetchedAt.HasValue && fetchedAt.Value > DateTime.UtcNow.AddDays(-1);
        }

        /// <summary>
        /// Retrieves a list of valid team tricodes from the raw team data stored in the database.
        /// </summary>
        /// <returns></returns>
        private List<string> GetValidTeamTriCodes(WorkerContext context)
        {
            var teamRaw = _db.RawApiResponses.FirstOrDefault(r => r.Endpoint == "team");
            if (teamRaw == null || string.IsNullOrWhiteSpace(teamRaw.ResponseJson))
            {               
                return new List<string>();
            }

            // if the raw team data cannot be deserialized into the expected DTO, log an error and return an empty list
            if (!teamRaw.ResponseJson.TryDeserializeSafe<NhlTeamRootDTO>( out var root, out _) || root?.Data == null)
            {                
                return new List<string>();
            }

            return root.Data
                .Where(t => t.TriCode != "TBD" && t.TriCode != "NHL")
                .Select(t => t.TriCode)
                .Distinct()
                .ToList();
        }
        #endregion
    }
}