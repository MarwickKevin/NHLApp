
using Microsoft.EntityFrameworkCore;
using NHLApp.Application.DTOs;
using NHLApp.Domain.Entities;
using NHLApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace NHLApp.Application.Services
{
    // TODO: Add logging and error handling to the TransformService methods for better observability and resilience.

    public class TransformService
    {
        private readonly NHLAppDbContext _db;

        public TransformService(NHLAppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Transforms raw season JSON payloads into structured Season entities in the database.
        /// </summary>
        /// <returns></returns>
        public async Task TransformSeasonsAsync()
        {
            var raw = await _db.RawApiResponses.FirstOrDefaultAsync(r => r.Endpoint == "season");
            if (raw == null || string.IsNullOrWhiteSpace(raw.ResponseJson)) return;

            // Deserialize the JSON array of season IDs into a list of integers
            List<int>? seasonIds = JsonSerializer.Deserialize<List<int>>(raw.ResponseJson);
            if (seasonIds == null) return;

            // Load existing season IDs into a HashSet for efficient lookups to avoid duplicates
            HashSet<int> existingSeasonIds = _db.Seasons.Select(s => s.SeasonId).ToHashSet();

            // Loop through the deserialized season IDs and add any new seasons to the database
            foreach (var seasonId in seasonIds)
            {
                if (existingSeasonIds.Contains(seasonId)) continue;

                _db.Seasons.Add(new Season
                {
                    SeasonId = seasonId,
                    StartYear = seasonId / 10000,
                    EndYear = seasonId % 10000
                });

                existingSeasonIds.Add(seasonId);
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Transforms raw team JSON payloads into structured Team and Franchise entities in the database.
        /// </summary>
        /// <returns></returns>
        public async Task TransformTeamsAsync()
        {            
            var teamRaws = _db.RawApiResponses.Where(r => r.Endpoint == "team");

            // Load existing structures into memory once to prevent duplicates
            HashSet<int> knownSeasonIds = _db.Seasons.Select(s => s.SeasonId).ToHashSet();
            HashSet<int> knownFranchiseIds = _db.Franchises.Select(f => f.FranchiseId).ToHashSet();
            HashSet<(int TeamId, int SeasonId)> knownTeamSeasons = _db.Teams
                .Select(t => new ValueTuple<int, int>(t.TeamId, t.SeasonId))
                .ToHashSet();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Stream through every chunk of team data in your staging table
            await foreach (var raw in teamRaws.AsAsyncEnumerable())
            {
                if (string.IsNullOrWhiteSpace(raw.ResponseJson)) 
                    continue;

                // Deserialize the JSON payload into a structured DTO object
                NhlTeamRootDTO? root = JsonSerializer.Deserialize<NhlTeamRootDTO>(raw.ResponseJson, options);
                if (root?.Data == null) 
                    continue;

                // Loop through each team item
                foreach (NhlTeamItemDTO item in root.Data)
                {
                    // Add new seasons to the Seasons table if they don't already exist
                    if (!knownSeasonIds.Contains(item.SeasonId))
                    {
                        int startYear = 0;
                        int endYear = 0;
                        string seasonStr = item.SeasonId.ToString();
                        if (seasonStr.Length == 8)
                        {
                            int.TryParse(seasonStr.Substring(0, 4), out startYear);
                            int.TryParse(seasonStr.Substring(4, 4), out endYear);
                        }

                        _db.Seasons.Add(new Season
                        {
                            SeasonId = item.SeasonId,
                            StartYear = startYear,
                            EndYear = endYear
                        });
                        knownSeasonIds.Add(item.SeasonId);
                    }

                    // Add new franchises to the Franchises table if they don't already exist
                    if (item.FranchiseId.HasValue && !knownFranchiseIds.Contains(item.FranchiseId.Value))
                    {
                        _db.Franchises.Add(new Franchise
                        {
                            FranchiseId = item.FranchiseId.Value,
                            Name = item.FullName
                        });
                        knownFranchiseIds.Add(item.FranchiseId.Value);
                    }

                    // Add new team-season relationships to the Teams table if they don't already exist
                    if (knownTeamSeasons.Contains((item.Id, item.SeasonId))) 
                        continue;

                    _db.Teams.Add(new Team
                    {
                        TeamId = item.Id,
                        SeasonId = item.SeasonId,
                        FranchiseId = item.FranchiseId,
                        FullName = item.FullName,
                        TriCode = item.TriCode,
                        RawTriCode = item.RawTricode,
                        LeagueId = item.LeagueId
                    });
                    knownTeamSeasons.Add((item.Id, item.SeasonId));
                }
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Transforms raw roster JSON payloads into structured Player entities in the database.
        /// </summary>
        /// <returns></returns>
        public async Task TransformPlayersAsync()
        {
            // Fetch all raw roster JSON payloads from staging
            var rosterRaws = _db.RawApiResponses.Where(r => r.Endpoint == "roster");

            // Load existing tracking sets to avoid duplicates and round-trips
            HashSet<int> knownPlayerIds = _db.Players.Select(p => p.PlayerId).ToHashSet();

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Loop through each raw roster payload
            await foreach (var raw in rosterRaws.AsAsyncEnumerable())                       /// *AsAsyncEnumerable() here instead of ToListAsync to avoid loading all data into memory at once*
            {
                if (string.IsNullOrWhiteSpace(raw.ResponseJson)) 
                    continue;

                // Automatically deserialize the entire nested structure using the DTO
                NhlRosterRootDTO? rosterData = JsonSerializer.Deserialize<NhlRosterRootDTO>(raw.ResponseJson, jsonOptions);
                if (rosterData == null) 
                    continue;

                // Flatten the three positional lists into a single collection
                IEnumerable<NhlPlayerDTO> allPlayers = (rosterData.Forwards ?? Enumerable.Empty<NhlPlayerDTO>())
                     .Concat(rosterData.Defensemen ?? Enumerable.Empty<NhlPlayerDTO>())
                     .Concat(rosterData.Goalies ?? Enumerable.Empty<NhlPlayerDTO>());

                // Loop through each player DTO and add new players to the Players table if they don't already exist
                foreach (NhlPlayerDTO playerDto in allPlayers)
                {
                    if (knownPlayerIds.Contains(playerDto.Id))
                        continue;

                    _db.Players.Add(new Player
                    {
                        PlayerId = playerDto.Id,
                        FirstName = playerDto.FirstName.Default,
                        LastName = playerDto.LastName.Default,
                        Position = playerDto.PositionCode,
                        ShootsCatches = playerDto.ShootsCatches ?? string.Empty,
                        HeightInCentimeters = playerDto.HeightInCentimeters,
                        WeightInKilograms = playerDto.WeightInKilograms,
                        BirthDate = playerDto.BirthDate != null ? DateOnly.Parse(playerDto.BirthDate) : null,
                        BirthCity = playerDto.BirthCity?.Default,
                        BirthCountry = playerDto.BirthCountry
                    });

                    knownPlayerIds.Add(playerDto.Id);
                }
            }
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Transforms raw roster JSON payloads into structured TeamRoster relationship entities in the database.
        /// </summary>
        /// <returns></returns>
        public async Task TransformRostersAsync()
        {
            // Fetch all raw seasonal roster payloads from staging
            var rosterRaws = _db.RawApiResponses.Where(r => r.Endpoint == "roster");        /// *AsAsyncEnumerable() in loop to save memory*

            // Load existing TeamRoster relationships into memory to avoid duplicates and round-trips
            HashSet<(int TeamId, int PlayerId, int SeasonId)> existingRosters = _db.TeamRosters
                .Select(tr => ValueTuple.Create(tr.TeamId, tr.PlayerId, tr.SeasonId))
                .ToHashSet();
            HashSet<int> validPlayerIds = _db.Players.Select(p => p.PlayerId).ToHashSet();
            HashSet<int> validSeasonIds = _db.Seasons.Select(s => s.SeasonId).ToHashSet();

            // Create a lookup dictionary for team tri-codes and season IDs to their corresponding TeamId for quick access
            Dictionary<(string TriCode, int SeasonId), int> teamLookup = _db.Teams
                .AsEnumerable() // Pull evaluation into memory to safely use GroupBy/ValueTuple
                .GroupBy(t => new ValueTuple<string, int>(t.TriCode, t.SeasonId))
                .ToDictionary(g => g.Key, g => g.First().TeamId);

            JsonSerializerOptions jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            await foreach (RawApiResponse raw in rosterRaws.AsAsyncEnumerable())            /// *AsAsyncEnumerable() here instead of ToListAsync to avoid loading all data into memory at once*
            {
                // Skip if the EntityId is not in the expected format ("roster-TEAMCODE-SEASONID")
                string[] keyParts = raw.EntityId.Split('-');
                if (keyParts.Length < 3) 
                    continue;

                // Extract the team tri-code and season ID from the EntityId
                int totalParts = keyParts.Length;               
                
                string teamTriCode = keyParts[totalParts - 2].Trim().ToUpper();
                if (!int.TryParse(keyParts[totalParts - 1], out int seasonId)) 
                    continue;


                // --- TEMPORARY DEBUGGING BLOCK ---
                var hasTeam = teamLookup.TryGetValue((teamTriCode, seasonId), out int debugTeamId);
                var hasSeason = validSeasonIds.Contains(seasonId);

                if (!hasTeam || !hasSeason)
                {
                    Console.WriteLine($"[MISSING] Key Parts: '{raw.EntityId}' -> Parsed TriCode: '{teamTriCode}', Season: {seasonId}");
                    Console.WriteLine($"   -> Match in teamLookup? {hasTeam} (Found TeamId: {(hasTeam ? debugTeamId : "N/A")})");
                    Console.WriteLine($"   -> Match in validSeasonIds? {hasSeason}");

                    // Let's check if the team code exists under a different casing or with spaces
                    var similarTeams = teamLookup.Keys.Where(k => k.SeasonId == seasonId).Select(k => k.TriCode);
                    Console.WriteLine($"   -> Available TriCodes for season {seasonId} in DB: {string.Join(", ", similarTeams)}");
                }
                // ---------------------------------


                // Ensure the team and season exist in our database core tables before processing the roster
                if (!teamLookup.TryGetValue((teamTriCode, seasonId), out int teamId) || !validSeasonIds.Contains(seasonId))
                    continue;

                // Automatically deserialize the entire nested structure using the DTO
                NhlRosterRootDTO? rosterData = JsonSerializer.Deserialize<NhlRosterRootDTO>(raw.ResponseJson, jsonOptions);

                // Skip if the roster data is null
                if (rosterData == null) 
                    continue;

                // Flatten the three positional lists into a single collection for processing
                IEnumerable<NhlPlayerDTO> allPlayers = (rosterData.Forwards ?? Enumerable.Empty<NhlPlayerDTO>())
                .Concat(rosterData.Defensemen ?? Enumerable.Empty<NhlPlayerDTO>())
                .Concat(rosterData.Goalies ?? Enumerable.Empty<NhlPlayerDTO>());

                // Loop through each player DTO and add new TeamRoster relationships to the TeamRosters table if they don't already exist
                foreach (NhlPlayerDTO playerDto in allPlayers)
                {                    
                    if (!validPlayerIds.Contains(playerDto.Id) || existingRosters.Contains((teamId, playerDto.Id, seasonId)))
                        continue;

                    _db.TeamRosters.Add(new TeamRosters
                    {
                        TeamId = teamId,
                        PlayerId = playerDto.Id,
                        SeasonId = seasonId
                    });
                                        
                    existingRosters.Add((teamId, playerDto.Id, seasonId));
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}