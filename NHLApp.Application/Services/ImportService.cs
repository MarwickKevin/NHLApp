using NHLApp.Domain.Entities;
using NHLApp.Domain.Interfaces;
using NHLApp.Infrastructure.Data;
using System.Text.Json;
using NHLApp.Application.DTOs;
using Microsoft.Extensions.Logging;
using NHLApp.Application.Extensions;

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

        public ImportService(INHLApiClient nhlClient, NHLAppDbContext db, ILogger<ImportService> logger)
        {
            _nhlClient = nhlClient;
            _db = db;
            this._logger = logger;
        }

        // Constants for API throttling to avoid hitting the NHL API too quickly
        private const int ApiThrottlingDelay = 150;

        /// <summary>
        /// Imports the latest seasons from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportSeasonsAsync()
        {
            // Check if the seasons have already been imported
            var existing = _db.RawApiResponses
                .FirstOrDefault(r => r.Endpoint == "season");

            // If the seasons have already been imported within the last 24 hours, skip (hash comparison could be used here instead of timestamp)
            if (existing != null && existing.FetchedAt > DateTime.UtcNow.AddDays(-1))
                return;
            try
            {
                // Fetch seasons from the NHL API
                var json = await _nhlClient.GetSeasonsAsync();

                // Update the existing record if it exists, otherwise insert a new record
                if (existing != null)
                {
                    // Only update if the JSON content actually changed
                    if (existing.ResponseJson != json)
                    {
                        existing.ResponseJson = json;
                        existing.FetchedAt = DateTime.UtcNow;
                        await _db.SaveChangesAsync();
                    }
                }
                else
                {
                    _db.RawApiResponses.Add(new RawApiResponse
                    {
                        Endpoint = "season",
                        EntityId = "all",
                        ResponseJson = json,
                        FetchedAt = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import endpoint {Endpoint} for entity {EntityId}.", "season", "all");
            }
        }

        /// <summary>
        /// Imports the latest teams from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportTeamsAsync()
        {
            // Check if the teams have already been imported
            var existing = _db.RawApiResponses
                .FirstOrDefault(r => r.Endpoint == "team");

            // If the teams have already been imported within the last 24 hours, skip (hash comparison could be used here instead of timestamp)
            if (existing != null && existing.FetchedAt > DateTime.UtcNow.AddDays(-1))
                return;

            try
            {
                // Fetch the latest teams from the NHL API
                var json = await _nhlClient.GetTeamsAsync();

                // Update the existing record if it exists, otherwise insert a new record
                if (existing != null)
                {
                    // Only update if the JSON content actually changed
                    if (existing.ResponseJson != json)
                    {
                        existing.ResponseJson = json;
                        existing.FetchedAt = DateTime.UtcNow;
                        await _db.SaveChangesAsync();
                    }
                }
                else
                {
                    _db.RawApiResponses.Add(new RawApiResponse
                    {
                        Endpoint = "team",
                        EntityId = "all",
                        ResponseJson = json,
                        FetchedAt = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import endpoint {Endpoint} for entity {EntityId}.", "team", "all");
            }
        }

        /// <summary>
        /// Imports the roster seasons for each team from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportRosterSeasonsAsync()
        {
            // Check if the team data has already been imported
            var teamRaw = _db.RawApiResponses.FirstOrDefault(r => r.Endpoint == "team");
            if (teamRaw == null || string.IsNullOrWhiteSpace(teamRaw.ResponseJson))
            {
                _logger.LogError("Cannot import roster seasons because raw team data has not been imported yet.");
                return;
            }

            // Deserialize the raw team data to extract the triCodes for each team
            if (!teamRaw.ResponseJson.TryDeserializeSafe<NhlTeamRootDTO>(_logger, out var root, "raw team data", out _) || root?.Data == null)
            {
                return;
            }

            var triCodes = root.Data
                .Where(t => t.TriCode != "TBD" && t.TriCode != "NHL")
                .Select(t => t.TriCode)
                .Distinct()
                .ToList();

            // Load existing records into memory to avoid N+1 queries
            var existingRecords = _db.RawApiResponses
                .Where(r => r.Endpoint == "roster-seasons")
                .Select(r => new { r.EntityId, r.FetchedAt })
                .ToDictionary(r => r.EntityId, r => r.FetchedAt);

            // Loop through each team and import the roster seasons data
            bool hasChanges = false;
            foreach (var triCode in triCodes)
            {
                // Check if the roster seasons for this team have already been imported
                var key = $"roster-seasons-{triCode}";
                existingRecords.TryGetValue(key, out var fetchedAt);

                // If the roster seasons for this team have already been imported within the last 24 hours, skip (hash comparison could be used here instead of timestamp)
                if (fetchedAt > DateTime.UtcNow.AddDays(-1))
                    continue;

                // Fetch the roster seasons for this team from the NHL API
                try
                {
                    // Fetch the roster seasons for this team from the NHL API
                    var json = await _nhlClient.GetTeamRosterSeasonsAsync(triCode);

                    // Check if the record already exists in the local context or database
                    var dbRecord = _db.RawApiResponses.Local.FirstOrDefault(r => r.EntityId == key)
                                            ?? _db.RawApiResponses.FirstOrDefault(r => r.EntityId == key);

                    // Update the existing record if it exists, otherwise insert a new record
                    if (dbRecord != null)
                    {
                        if (dbRecord.ResponseJson != json)
                        {
                            dbRecord.ResponseJson = json;
                            dbRecord.FetchedAt = DateTime.UtcNow;
                            hasChanges = true;
                        }
                    }
                    else
                    {
                        _db.RawApiResponses.Add(new RawApiResponse
                        {
                            Endpoint = "roster-seasons",
                            EntityId = key,
                            ResponseJson = json,
                            FetchedAt = DateTime.UtcNow
                        });
                        hasChanges = true;
                    }
                    // Throttle delay
                    await Task.Delay(ApiThrottlingDelay);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to import endpoint {Endpoint} for entity {EntityId}.", "roster-seasons", triCode);
                }
            }
            if (hasChanges)
            {
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Imports the rosters for each team and season from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportRostersAsync()
        {
            // Check if the team data has been imported
            var teamRaw = _db.RawApiResponses.FirstOrDefault(r => r.Endpoint == "team");
            if (teamRaw == null || string.IsNullOrWhiteSpace(teamRaw.ResponseJson))
            {
                _logger.LogError("Cannot import rosters because raw team data has not been imported yet.");
                return;
            }

            // Deserialize the raw team data to extract the triCodes
            if (!teamRaw.ResponseJson.TryDeserializeSafe<NhlTeamRootDTO>(_logger, out var root, "team JSON", out _) || root?.Data == null)
            {
                return;
            }

            // Get the distinct triCodes for all teams, excluding placeholder codes like "TBD" and "NHL"
            var triCodes = root.Data
                .Where(t => t.TriCode != "TBD" && t.TriCode != "NHL")
                .Select(t => t.TriCode)
                .Distinct()
                .ToList();

            // Load existing roster records into memory to avoid N+1 queries
            var existingRosterRecords = _db.RawApiResponses
                .Where(r => r.Endpoint == "roster")
                .Select(r => new { r.EntityId, r.FetchedAt })
                .ToDictionary(r => r.EntityId, r => r.FetchedAt);

            bool hasChanges = false;
            // For each team, retrieve the roster seasons and then import the roster for each season
            foreach (var triCode in triCodes)
            {
                // Check if the roster seasons for this team have already been imported
                var rosterSeasonsRaw = _db.RawApiResponses
                    .FirstOrDefault(r => r.EntityId == $"roster-seasons-{triCode}");

                if (rosterSeasonsRaw == null)
                {
                    _logger.LogError("Error importing rosters for team {TriCode}: Raw roster seasons data has not been imported yet.", triCode);
                    continue;
                }

                // Deserialize the JSON array of season IDs
                if (!rosterSeasonsRaw.ResponseJson.TryDeserializeSafe<List<int>>(_logger, out var seasonIds, $"roster seasons for team {triCode}", out _) || seasonIds == null)
                {
                    continue;
                }

                // Loop through each season for the team and import the roster data                
                foreach (var seasonId in seasonIds)
                {
                    var key = $"roster-{triCode}-{seasonId}";

                    // Check if the roster for this team and season has already been imported within the last 24 hours
                    existingRosterRecords.TryGetValue(key, out var fetchedAt);
                    if (fetchedAt > DateTime.UtcNow.AddDays(-1))
                        continue;

                    // Fetch the roster for this team and season from the NHL API
                    try
                    {
                        var json = await _nhlClient.GetTeamRosterAsync(triCode, seasonId);

                        var dbRecord = _db.RawApiResponses.Local.FirstOrDefault(r => r.EntityId == key)
                                                    ?? _db.RawApiResponses.FirstOrDefault(r => r.EntityId == key);

                        // Update the existing record if it exists, otherwise insert a new record
                        if (dbRecord != null)
                        {
                            if (dbRecord.ResponseJson != json)
                            {
                                dbRecord.ResponseJson = json;
                                dbRecord.FetchedAt = DateTime.UtcNow;
                                hasChanges = true;
                            }
                        }
                        else
                        {
                            _db.RawApiResponses.Add(new RawApiResponse
                            {
                                Endpoint = "roster",
                                EntityId = key,
                                ResponseJson = json,
                                FetchedAt = DateTime.UtcNow
                            });
                            hasChanges = true;
                        }
                        // Throttle delay
                        await Task.Delay(ApiThrottlingDelay);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to import endpoint {Endpoint} for entity {EntityId}.", "roster", $"{triCode}-{seasonId}");
                    }

                }
            }
            if (hasChanges)
            {
                await _db.SaveChangesAsync();
            }
        }
    }
}