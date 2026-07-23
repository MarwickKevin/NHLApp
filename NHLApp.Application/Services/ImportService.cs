using NHLApp.Domain.Entities;
using NHLApp.Domain.Interfaces;
using NHLApp.Infrastructure.Data;
using System.Text.Json;
using NHLApp.Application.DTOs;
using Microsoft.Extensions.Logging;
using NHLApp.Application.Extensions;
using System.Net;

namespace NHLApp.Application.Services
{
    // TODO: Add retry logic for API calls to handle transient failures and improve reliability    

    // TODO Make error handling consistent across all methods, including logging and exception throwing.

    // TODO: Use hash comparison for JSON content to determine if an update is necessary, instead of relying solely on timestamps

    public class ImportService
    {
        private readonly INHLApiClient _nhlClient;
        private readonly NHLAppDbContext _db;
        private readonly ILogger<ImportService> _logger;
        private readonly RawDataStore _rawDataStore;

        public ImportService(INHLApiClient nhlClient, NHLAppDbContext db, ILogger<ImportService> logger, RawDataStore rawDataStore)
        {
            _nhlClient = nhlClient;
            _db = db;
            this._logger = logger;
            _rawDataStore = rawDataStore;
        }

        // Constants for API throttling to avoid hitting the NHL API too quickly
        private const int ApiThrottlingDelay = 150;

        /// <summary>
        /// Imports the latest seasons from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportSeasonsAsync()
        {
            // Check if the seasons have already been imported within the last 24 hours
            await ProcessImportAsync(
                endpoint: "season",
                entityId: "all",
                fetchApiAsync: () => _nhlClient.GetSeasonsAsync());
        }

        /// <summary>
        /// Imports the latest teams from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportTeamsAsync()
        {
            await ProcessImportAsync(
                endpoint: "team",
                entityId: "all",
                fetchApiAsync: () => _nhlClient.GetTeamsAsync());
        }

        /// <summary>
        /// Imports the roster seasons for each team from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportRosterSeasonsAsync()
        {
            var triCodes = GetValidTeamTriCodes();
            if (!triCodes.Any())
                return;

            await ProcessImportAsync(
                items: triCodes,
                endpoint: "roster-seasons",
                entityIdSelector: triCode => triCode,
                fetchApiAsync: triCode => _nhlClient.GetTeamRosterSeasonsAsync(triCode));
        }

        /// <summary>
        /// Imports the rosters for each team and season from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportRostersAsync()
        {
            // Extract team tricodes from the raw team data
            var triCodes = GetValidTeamTriCodes();
            if (!triCodes.Any())
                return;

            // For each team, retrieve the roster seasons and then import the roster for each season
            foreach (var triCode in triCodes)
            {
                var rosterSeasonsRaw = _db.RawApiResponses
                    .FirstOrDefault(r => r.EntityId == $"roster-seasons-{triCode}");

                if (rosterSeasonsRaw == null || !rosterSeasonsRaw.ResponseJson.TryDeserializeSafe<List<int>>(_logger, out var seasonIds, $"roster seasons for team {triCode}", out _) || seasonIds == null)
                {
                    continue;
                }

                // Process each season for the team, fetching the roster data and storing it in the database
                await ProcessImportAsync(
                    items: seasonIds,
                    endpoint: "roster",
                    entityIdSelector: seasonId => $"{triCode}-{seasonId}",
                    fetchApiAsync: seasonId => _nhlClient.GetTeamRosterAsync(triCode, seasonId));
            }
        }

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
        private List<string> GetValidTeamTriCodes()
        {
            var teamRaw = _db.RawApiResponses.FirstOrDefault(r => r.Endpoint == "team");
            if (teamRaw == null || string.IsNullOrWhiteSpace(teamRaw.ResponseJson))
            {
                _logger.LogError("Cannot process because raw team data has not been imported yet.");
                return new List<string>();
            }

            if (!teamRaw.ResponseJson.TryDeserializeSafe<NhlTeamRootDTO>(_logger, out var root, "team JSON", out _) || root?.Data == null)
            {
                return new List<string>();
            }

            return root.Data
                .Where(t => t.TriCode != "TBD" && t.TriCode != "NHL")
                .Select(t => t.TriCode)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Processes a single item, fetching data from the NHL API and storing it in the database if it hasn't been fetched recently.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="entityId"></param>
        /// <param name="fetchApiAsync"></param>
        /// <returns></returns>
        private async Task ProcessImportAsync(
            string endpoint,
            string entityId,
            Func<Task<string>> fetchApiAsync)
        {
            var key = $"{endpoint}-{entityId}";

            var existing = _db.RawApiResponses
                .FirstOrDefault(r => r.Endpoint == endpoint && r.EntityId == entityId);

            if (IsFresh(existing?.FetchedAt))
                return;

            try
            {
                var json = await fetchApiAsync();
                await _rawDataStore.SaveOrUpdateAsync(endpoint, key, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import endpoint {Endpoint} for entity {EntityId}.", endpoint, entityId);
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
        private async Task ProcessImportAsync<T>(
            IEnumerable<T> items,
            string endpoint,
            Func<T, string> entityIdSelector,
            Func<T, Task<string>> fetchApiAsync)
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
                    continue;

                try
                {
                    var json = await fetchApiAsync(item);

                    await _rawDataStore.SaveOrUpdateAsync(endpoint, key, json);

                    await Task.Delay(ApiThrottlingDelay);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to import endpoint {Endpoint} for entity {EntityId}.", endpoint, entityId);
                }
            }
        }
    }
}