using NHLApp.Domain.Entities;
using NHLApp.Domain.Interfaces;
using NHLApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NHLApp.Domain;

namespace NHLApp.Application.Services
{
    // TODO: Add logging and error handling to this service to improve maintainability and debuggability
    // TODO: Consider adding caching to reduce the number of API calls and improve performance
    // TODO: Add retry logic for API calls to handle transient failures and improve reliability   


    public class ImportService
    {
        private readonly INHLApiClient _nhlClient;
        private readonly NHLAppDbContext _db;

        public ImportService(INHLApiClient nhlClient, NHLAppDbContext db)
        {
            _nhlClient = nhlClient;
            _db = db;
        }

        // Constants for API throttling to avoid hitting the NHL API too quickly
        private const int ApiThrottlingDelay = 50;

        // Counters for monitoring the number of saves to the database and API calls made during the import process
        private int SavesToDB = 0;
        private int ApiCallsMade = 0;

        /// <summary>
        /// Imports the latest seasons from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportSeasonsAsync()
        {
            // Check if the seasons have already been imported
            var existing = _db.RawApiResponses
                .FirstOrDefault(r => r.Endpoint == "season");

            // If the seasons have already been fetched within the last 24 hours, skip the import to avoid unnecessary API calls
            if (existing != null && existing.FetchedAt > DateTime.UtcNow.AddDays(-1))
                return;

            // Retrieve the latest seasons from the NHL API
            var json = await _nhlClient.GetSeasonsAsync();
            ApiCallsMade++;
            Console.WriteLine("Api call made = " + ApiCallsMade);

            if (existing != null)
            {
                // Only update if the JSON content actually changed to avoid useless DB writes
                if (existing.ResponseJson != json)
                {
                    existing.ResponseJson = json;
                    existing.FetchedAt = DateTime.UtcNow;
                }
            }
            else
            {
                // Insert a brand new staging record
                _db.RawApiResponses.Add(new RawApiResponse
                {
                    Endpoint = "season",
                    EntityId = "all",
                    ResponseJson = json,
                    FetchedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            // Increment the linesProcessed counter and log it to the console for monitoring purposes
            SavesToDB++;
            Console.WriteLine("Saves to DB = " + SavesToDB);
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

            // If the teams have already been fetched within the last 24 hours, skip the import to avoid unnecessary API calls
            if (existing != null && existing.FetchedAt > DateTime.UtcNow.AddDays(-1))
                return;

            // Retrieve the latest teams from the NHL API
            var json = await _nhlClient.GetTeamsAsync();
            ApiCallsMade++;
            Console.WriteLine("Api call made = " + ApiCallsMade);

            // Store the raw response in the database
            _db.RawApiResponses.Add(new RawApiResponse
            {
                Endpoint = "team",
                EntityId = "all",
                ResponseJson = json,
                FetchedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            // Increment the linesProcessed counter and log it to the console for monitoring purposes
            SavesToDB++;
            Console.WriteLine("Saves to DB = " + SavesToDB);
        }

        /// <summary>
        /// Imports the roster seasons for each team from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportRosterSeasonsAsync()
        {
            var teams = _db.Teams
                .Where(t => t.TriCode != "TBD" && t.TriCode != "NHL")
                .ToList();

            bool hasChanges = false;

            // Loop through each team and fetch the roster seasons data
            foreach (var team in teams)
            {
                var key = $"roster-seasons-{team.TriCode}";

                // Check if the roster seasons for this team have already been imported
                var existing = _db.RawApiResponses
                    .FirstOrDefault(r => r.EntityId == key);

                // If the roster seasons for this team have already been fetched within the last 24 hours, skip
                if (existing != null && existing.FetchedAt > DateTime.UtcNow.AddDays(-1))
                    continue;

                // Retrieve the roster seasons for this team from the NHL API
                try
                {
                    var json = await _nhlClient.GetTeamRosterSeasonsAsync(team.TriCode);
                    ApiCallsMade++;
                    Console.WriteLine("Api call made = " + ApiCallsMade);

                    // Store the raw response in the database, either by updating an existing record or inserting a new one
                    if (existing != null)
                    {
                        if (existing.ResponseJson != json)
                        {
                            existing.ResponseJson = json;
                            existing.FetchedAt = DateTime.UtcNow;
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
                    Console.WriteLine($"Erreur roster-seasons {team.TriCode}: {ex.Message}");
                }
            }

            // Save tracking details if any additions/updates occurred during the loop for all teams
            if (hasChanges)
            {
                await _db.SaveChangesAsync();
                SavesToDB++;
                Console.WriteLine("Saves to DB = " + SavesToDB);
            }
        }

        /// <summary>
        /// Imports the rosters for each team and season from the NHL API and stores them in the database.
        /// </summary>
        /// <returns></returns>
        public async Task ImportRostersAsync()
        {
            // Retrieve all teams from the database, excluding placeholder teams
            var teams = _db.Teams
                .Where(t => t.TriCode != "TBD" && t.TriCode != "NHL")
                .ToList();

            // For each team, retrieve the roster seasons and then fetch the roster for each season
            foreach (var team in teams)
            {
                var rosterSeasonsRaw = _db.RawApiResponses
                    .FirstOrDefault(r => r.EntityId == $"roster-seasons-{team.TriCode}");

                if (rosterSeasonsRaw == null) continue;

                // Deserialize the JSON array of season IDs into a list of integers
                var seasonIds = JsonSerializer.Deserialize<List<int>>(rosterSeasonsRaw.ResponseJson)!;

                bool hasChanges = false;

                // Loop through each season for the team and fetch the roster data
                foreach (var seasonId in seasonIds)
                {
                    var key = $"roster-{team.TriCode}-{seasonId}";

                    // Check if the roster for this team and season has already been imported
                    var existing = _db.RawApiResponses.FirstOrDefault(r => r.EntityId == key);

                    if (existing != null && existing.FetchedAt > DateTime.UtcNow.AddDays(-1))
                        continue;

                    // Fetch the roster for this team and season from the NHL API and store it in the database
                    try
                    {
                        var json = await _nhlClient.GetTeamRosterAsync(team.TriCode, seasonId);
                        ApiCallsMade++;
                        Console.WriteLine("Api call made = " + ApiCallsMade);

                        // Store the raw response in the database, either by updating an existing record or inserting a new one
                        if (existing != null)
                        {
                            if (existing.ResponseJson != json)
                            {
                                existing.ResponseJson = json;
                                existing.FetchedAt = DateTime.UtcNow;
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
                        Console.WriteLine($"Erreur roster {team.TriCode} {seasonId}: {ex.Message}");
                    }
                }
                
                // Save tracking details if any additions/updates occurred during the loop for this team
                if (hasChanges)
                {
                    await _db.SaveChangesAsync();
                    SavesToDB++;
                    Console.WriteLine($"Saves to DB = {SavesToDB} (Batch saved for team {team.TriCode})");
                }
            }
        }
    }
}