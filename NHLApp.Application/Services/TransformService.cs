using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NHLApp.Application.DTOs;
using NHLApp.Application.Extensions;
using NHLApp.Domain.Entities;
using NHLApp.Infrastructure.Data;
using System.Linq.Expressions;

namespace NHLApp.Application.Services
{
    // TODO: Implement an IsProcessed flag on RawApiResponses to skip already-transformed staging records.
    // TODO: Make error handling consistent across all methods, including logging and exception throwing.

    public class TransformService
    {
        private readonly NHLAppDbContext _db;
        private readonly ILogger<TransformService> _logger;

        public TransformService(NHLAppDbContext db, ILogger<TransformService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Transforms raw season JSON payloads into structured Season entities in the database.
        /// </summary>
        /// <returns></returns>
        public async Task TransformSeasonsAsync()
        {
            // Fetch the raw season JSON payload
            var raw = await _db.RawApiResponses.FirstOrDefaultAsync(r => r.Endpoint == "season");
            if (raw == null || string.IsNullOrWhiteSpace(raw.ResponseJson))
            {
                _logger.LogError("TransformSeasons aborted: No raw season response found in staging.");
                return;
            }

            // Deserialize the JSON array of season IDs into a list of integers
            if (!raw.ResponseJson.TryDeserializeSafe<List<int>>(_logger, out var seasonIds, "raw season payload", out _) || seasonIds == null)
            {
                return;
            }

            // Load existing season IDs into a HashSet for efficient lookups to avoid duplicates
            HashSet<int> existingSeasonIds = (await _db.Seasons.Select(s => s.SeasonId).ToListAsync()).ToHashSet();

            // Loop through the deserialized season IDs and add any new seasons to the database
           
            foreach (var seasonId in seasonIds)
            {
                bool hasChanges = false;
                try
                {
                    if (existingSeasonIds.Contains(seasonId))
                        continue;

                    _db.Seasons.Add(new Season
                    {
                        SeasonId = seasonId,
                        StartYear = seasonId / 10000,
                        EndYear = seasonId % 10000
                    });

                    existingSeasonIds.Add(seasonId);
                    hasChanges = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to transform season ID {SeasonId}", seasonId);
                }
                if (hasChanges)
                {
                    await _db.SaveChangesAsync();
                    _db.ChangeTracker.Clear();
                }
            }           
        }

        /// <summary>
        /// Transforms raw team JSON payloads into structured Team and Franchise entities in the database.
        /// </summary>
        /// <returns></returns>
        public async Task TransformTeamsAsync()
        {
            // Fetch all raw team and roster-seasons JSON payloads
            var teamRaws = await _db.RawApiResponses.Where(r => r.Endpoint == "team").ToListAsync();
            var rosterSeasonsRaws = await _db.RawApiResponses.Where(r => r.Endpoint == "roster-seasons").ToListAsync();

            // Create a mapping of team tri-codes to their corresponding list of season IDs from the roster-seasons endpoint
            var tricodeToSeasons = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

            // Loop through each roster-seasons raw payload to build the mapping
            foreach (var rRaw in rosterSeasonsRaws)
            {
                // Extract the tri-code from the EntityId (expected format: "roster-seasons-TRICODE")
                var parts = rRaw.EntityId.Split('-');
                if (parts.Length < 3)
                {
                    _logger.LogError("TransformTeams aborted: Invalid EntityId format for roster-seasons response: '{EntityId}'.", rRaw.EntityId);
                    continue;
                }
                string triCode = parts.Last();

                if (string.IsNullOrWhiteSpace(rRaw.ResponseJson))
                {
                    _logger.LogError("TransformTeams skipping: ResponseJson is null or whitespace for roster-seasons EntityId '{EntityId}'.", rRaw.EntityId);
                    continue;
                }

                // Deserialize the JSON array of integers
                if (!rRaw.ResponseJson.TryDeserializeSafe<List<int>>(_logger, out var seasonsList, $"roster-seasons for {rRaw.EntityId}", out _))
                {
                    continue;
                }

                if (seasonsList != null)
                {
                    tricodeToSeasons[triCode] = seasonsList;
                }
            }

            // Load existing structures into memory once to prevent duplicates
            HashSet<int> knownSeasonIds = (await _db.Seasons.Select(s => s.SeasonId).ToListAsync()).ToHashSet();
            HashSet<int> knownFranchiseIds = (await _db.Franchises.Select(f => f.FranchiseId).ToListAsync()).ToHashSet();
            HashSet<(int TeamId, int SeasonId)> knownTeamSeasons = (await _db.Teams.Select(t => new ValueTuple<int, int>(t.TeamId, t.SeasonId)).ToListAsync()).ToHashSet();

            // Loop through each raw team payload and transform it into structured Team and Franchise entities
           
            foreach (var raw in teamRaws)
            {
                bool hasChanges = false;
                try
                {
                    if (string.IsNullOrWhiteSpace(raw.ResponseJson))
                    {
                        _logger.LogError("TransformTeams skipping: ResponseJson is null or whitespace for team EntityId '{EntityId}'.", raw.EntityId);
                        continue;
                    }

                    // Deserialize the JSON payload into a structured DTO object
                    if (!raw.ResponseJson.TryDeserializeSafe<NhlTeamRootDTO>(_logger, out var root, $"team payload {raw.EntityId}", out _) || root == null)
                    {
                        continue;
                    }

                    foreach (NhlTeamItemDTO item in root.Data ?? Enumerable.Empty<NhlTeamItemDTO>())
                    {
                        // Add new franchises if they don't exist
                        if (item.FranchiseId.HasValue && !knownFranchiseIds.Contains(item.FranchiseId.Value))
                        {
                            _db.Franchises.Add(new Franchise
                            {
                                FranchiseId = item.FranchiseId.Value,
                                Name = item.FullName
                            });
                            knownFranchiseIds.Add(item.FranchiseId.Value);
                            hasChanges = true;
                        }

                        // Find all historical seasons for this team using its TriCode from the roster-seasons mapping
                        if (tricodeToSeasons.TryGetValue(item.TriCode, out var validSeasons))
                        {
                            foreach (int seasonId in validSeasons)
                            {
                                // Add new seasons if they don't exist
                                if (!knownSeasonIds.Contains(seasonId))
                                {
                                    int startYear = 0;
                                    int endYear = 0;
                                    string seasonStr = seasonId.ToString();
                                    if (seasonStr.Length == 8)
                                    {
                                        int.TryParse(seasonStr.Substring(0, 4), out startYear);
                                        int.TryParse(seasonStr.Substring(4, 4), out endYear);
                                    }

                                    _db.Seasons.Add(new Season
                                    {
                                        SeasonId = seasonId,
                                        StartYear = startYear,
                                        EndYear = endYear
                                    });
                                    knownSeasonIds.Add(seasonId);
                                    hasChanges = true;
                                }

                                // Add new team-season relationships if they don't exist
                                if (knownTeamSeasons.Contains((item.Id, seasonId)))
                                    continue;

                                _db.Teams.Add(new Team
                                {
                                    TeamId = item.Id,
                                    SeasonId = seasonId,
                                    FranchiseId = item.FranchiseId,
                                    FullName = item.FullName,
                                    TriCode = item.TriCode,
                                    RawTriCode = item.RawTricode,
                                    LeagueId = item.LeagueId
                                });
                                knownTeamSeasons.Add((item.Id, seasonId));
                                hasChanges = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to transform team payload for EntityId {EntityId}", raw.EntityId);
                }
                if (hasChanges)
                {
                    await _db.SaveChangesAsync();
                    _db.ChangeTracker.Clear();
                }
            }            
        }

        /// <summary>
        /// Transforms raw roster JSON payloads into structured Player entities in the database.
        /// </summary>
        /// <returns></returns>
        public async Task TransformPlayersAsync()
        {
            // Fetch all raw roster JSON payloads from staging
            var rosterRaws = await _db.RawApiResponses.Where(r => r.Endpoint == "roster").ToListAsync();

            // Load existing tracking sets to avoid duplicates and round-trips
            HashSet<int> knownPlayerIds = (await _db.Players.Select(p => p.PlayerId).ToListAsync()).ToHashSet();

            // Loop through each raw roster payload     
            
            foreach (var raw in rosterRaws)
            {
                bool hasChanges = false;
                try
                {
                    if (string.IsNullOrWhiteSpace(raw.ResponseJson))
                    {
                        _logger.LogError("TransformPlayers skipping: ResponseJson is null or whitespace for roster EntityId '{EntityId}'.", raw.EntityId);
                        continue;
                    }

                    // Automatically deserialize the entire nested structure using the DTO                
                    if (!raw.ResponseJson.TryDeserializeSafe<NhlRosterRootDTO>(_logger, out var rosterData, $"roster payload {raw.EntityId}", out _) || rosterData == null)
                    {
                        continue;
                    }

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
                        hasChanges = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to transform player roster payload for EntityId {EntityId}", raw.EntityId);
                }
                if (hasChanges)
                {
                    await _db.SaveChangesAsync();
                    _db.ChangeTracker.Clear();
                }
            }
            
        }

        /// <summary>
        /// Transforms raw roster JSON payloads into structured TeamRoster relationship entities in the database.
        /// </summary>
        /// <returns></returns>
        public async Task TransformRostersAsync()
        {
            // Fetch all raw seasonal roster payloads from staging
            var rosterRaws = await _db.RawApiResponses.Where(r => r.Endpoint == "roster").ToListAsync();

            // Load existing TeamRoster relationships into memory to avoid duplicates and round-trips
            HashSet<(int TeamId, int PlayerId, int SeasonId)> existingRosters = (await _db.TeamRosters.Select(tr => ValueTuple.Create(tr.TeamId, tr.PlayerId, tr.SeasonId)).ToListAsync()).ToHashSet();
            HashSet<int> validPlayerIds = (await _db.Players.Select(p => p.PlayerId).ToListAsync()).ToHashSet();
            HashSet<int> validSeasonIds = (await _db.Seasons.Select(s => s.SeasonId).ToListAsync()).ToHashSet();

            // Create a lookup dictionary for team tri-codes and season IDs to their corresponding TeamId for quick access
            Dictionary<(string TriCode, int SeasonId), int> teamLookup = _db.Teams
                .AsEnumerable() // Pull evaluation into memory to safely use GroupBy/ValueTuple
                .GroupBy(t => new ValueTuple<string, int>(t.TriCode, t.SeasonId))
                .ToDictionary(g => g.Key, g => g.First().TeamId);
                       
            foreach (RawApiResponse raw in rosterRaws)
            {
                bool hasChanges = false;
                try
                {                    
                    // Skip if the EntityId is not in the expected format ("roster-TEAMCODE-SEASONID")
                    string[] keyParts = raw.EntityId.Split('-');
                    if (keyParts.Length < 3)
                    {
                        _logger.LogError("TransformRosters skipping: Invalid EntityId format for team roster response: '{EntityId}'.", raw.EntityId);
                        continue;
                    }

                    // Extract the team tri-code and season ID from the EntityId
                    int totalParts = keyParts.Length;

                    string teamTriCode = keyParts[totalParts - 2].Trim().ToUpper();
                    if (!int.TryParse(keyParts[totalParts - 1], out int seasonId))
                    {
                        _logger.LogError("TransformRosters skipping: Failed to parse season ID from EntityId format: '{EntityId}'.", raw.EntityId);
                        continue;
                    }

                    // Ensure the team and season exist in our database core tables before processing the roster
                    if (!teamLookup.TryGetValue((teamTriCode, seasonId), out int teamId) || !validSeasonIds.Contains(seasonId))
                        continue;

                    // Automatically deserialize the entire nested structure using the DTO
                    if (!raw.ResponseJson.TryDeserializeSafe<NhlRosterRootDTO>(_logger, out var rosterData, $"team roster payload {raw.EntityId}", out _) || rosterData == null)
                    {
                        continue;
                    }

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
                        hasChanges = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to transform team roster relationship for EntityId {EntityId}", raw.EntityId);
                }
                if (hasChanges)
                {
                    await _db.SaveChangesAsync();
                    _db.ChangeTracker.Clear();
                }
            }
           
        }
    }
}