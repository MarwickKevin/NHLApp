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
    // TODO: Improve commenting and documentation for each method, including parameter descriptions and return values.
    // TODO: Implement an Upsert mechanism (or update logic) to refresh existing entities if source JSON data or schema columns change.

    public class TransformService
    {
        private readonly NHLAppDbContext _db;
        private readonly ILogger<TransformService> _logger;

        public TransformService(NHLAppDbContext db, ILogger<TransformService> logger)
        {
            _db = db;
            _logger = logger;
        }

        #region RAW API RESPONSE TRANSFORMATION

        /// <summary>
        /// Transforms raw season JSON payloads into structured Season entities in the database, extracting start and end years from the season ID.
        /// </summary>
        /// <returns></returns>
        public async Task TransformSeasonsAsync()
        {
            await ProcessGenericCollectionAsync<List<int>, int, int, Season>(
                endpoint: "season",
                operationName: "TransformSeasons",
                itemsSelector: seasonIds => seasonIds,
                keySelector: seasonId => seasonId,
                mapEntity: seasonId =>
                {
                    var (startYear, endYear) = ParseSeasonYears(seasonId);
                    return new Season { SeasonId = seasonId, StartYear = startYear, EndYear = endYear };
                },
                dbSet: _db.Seasons);
        }

        /// <summary>
        /// Transforms raw team JSON payloads into structured Franchise and Team entities in the database, linking teams to their franchises and seasons.
        /// </summary>
        /// <returns></returns>        
        public async Task TransformTeamsAsync()
        {
            // Build the mapping of team tri-codes to their corresponding list of season IDs using a helper
            var tricodeToSeasons = await BuildTricodeToSeasonsMappingAsync();

            // Load existing caches in memory
            HashSet<int> knownFranchiseIds = await GetKnownKeysAsync<Franchise, int>(f => f.FranchiseId);
            HashSet<int> knownSeasonIds = await GetKnownKeysAsync<Season, int>(s => s.SeasonId);
            HashSet<(int TeamId, int SeasonId)> knownTeamSeasons = await GetKnownKeysAsync<Team, (int TeamId, int SeasonId)>(t => ValueTuple.Create(t.TeamId, t.SeasonId));

            // Process each team item from the raw API responses, adding new franchises, seasons, and team-season relationships as needed
            await ProcessEndpointCollectionAsync<NhlTeamRootDTO, NhlTeamItemDTO>(
                endpoint: "team",
                operationName: "TransformTeams",
                itemsSelector: root => root.Data ?? Enumerable.Empty<NhlTeamItemDTO>(),
                processItemAsync: teamDto =>
                {
                    bool itemChanges = false;

                    // Add new franchises if they don't exist
                    if (teamDto.FranchiseId.HasValue && knownFranchiseIds.Add(teamDto.FranchiseId.Value))
                    {
                        _db.Franchises.Add(new Franchise { FranchiseId = teamDto.FranchiseId.Value, Name = teamDto.FullName });
                        itemChanges = true;
                    }

                    // Find all historical seasons for this team using its TriCode from the roster-seasons mapping
                    if (tricodeToSeasons.TryGetValue(teamDto.TriCode, out var validSeasons))
                    {
                        foreach (int seasonId in validSeasons)
                        {
                            // Add new seasons if they don't exist
                            if (!knownSeasonIds.Contains(seasonId))
                            {
                                var (startYear, endYear) = ParseSeasonYears(seasonId);
                                _db.Seasons.Add(new Season { SeasonId = seasonId, StartYear = startYear, EndYear = endYear });
                                knownSeasonIds.Add(seasonId);
                                itemChanges = true;
                            }

                            // Add new team-season relationships if they don't exist
                            if (knownTeamSeasons.Contains((teamDto.Id, seasonId)))
                                continue;

                            _db.Teams.Add(new Team
                            {
                                TeamId = teamDto.Id,
                                SeasonId = seasonId,
                                FranchiseId = teamDto.FranchiseId,
                                FullName = teamDto.FullName,
                                TriCode = teamDto.TriCode,
                                RawTriCode = teamDto.RawTricode,
                                LeagueId = teamDto.LeagueId
                            });
                            knownTeamSeasons.Add((teamDto.Id, seasonId));
                            itemChanges = true;
                        }
                    }
                    return Task.FromResult(itemChanges);
                });
        }

        /// <summary>
        /// Transforms raw player JSON payloads into structured Player entities in the database, extracting relevant player information.
        /// </summary>
        /// <returns></returns>
        public async Task TransformPlayersAsync()
        {
            await ProcessGenericCollectionAsync<NhlRosterRootDTO, NhlPlayerDTO, int, Player>(
                endpoint: "roster",
                operationName: "TransformPlayers",
                itemsSelector: root => (root.Forwards ?? Enumerable.Empty<NhlPlayerDTO>())
                    .Concat(root.Defensemen ?? Enumerable.Empty<NhlPlayerDTO>())
                    .Concat(root.Goalies ?? Enumerable.Empty<NhlPlayerDTO>()),
                keySelector: playerDto => playerDto.Id,
                mapEntity: playerDto => new Player
                {
                    PlayerId = playerDto.Id,
                    FirstName = playerDto.FirstName?.Default ?? string.Empty,
                    LastName = playerDto.LastName?.Default ?? string.Empty,
                    Position = playerDto.PositionCode,
                    ShootsCatches = playerDto.ShootsCatches ?? string.Empty,
                    HeightInCentimeters = playerDto.HeightInCentimeters,
                    WeightInKilograms = playerDto.WeightInKilograms,
                    BirthDate = playerDto.BirthDate != null ? DateOnly.Parse(playerDto.BirthDate) : null,
                    BirthCity = playerDto.BirthCity?.Default,
                    BirthCountry = playerDto.BirthCountry,
                    SweaterNumber = playerDto.SweaterNumber,
                    HeightInInches = playerDto.HeightInInches,
                    WeightInPounds = playerDto.WeightInPounds,
                    Headshot = playerDto.Headshot,
                    BirthStateProvince = playerDto.BirthStateProvince?.Default
                },
                dbSet: _db.Players);
        }

        /// <summary>
        /// Transforms raw roster JSON payloads into structured TeamRosters relationships in the database, linking players to teams for specific seasons.
        /// </summary>
        /// <returns></returns>
        public async Task TransformRostersAsync()
        {
            // Load existing TeamRoster relationships into memory to avoid duplicates and round-trips            
            HashSet<(int TeamId, int PlayerId, int SeasonId)> existingRosters = await GetKnownKeysAsync<TeamRosters, (int TeamId, int PlayerId, int SeasonId)>(tr => ValueTuple.Create(tr.TeamId, tr.PlayerId, tr.SeasonId));
            HashSet<int> validPlayerIds = await GetKnownKeysAsync<Player, int>(p => p.PlayerId);
            HashSet<int> validSeasonIds = await GetKnownKeysAsync<Season, int>(s => s.SeasonId);

            // Create a lookup dictionary for team tri-codes and season IDs to their corresponding TeamId for quick access
            Dictionary<(string TriCode, int SeasonId), int> teamLookup = _db.Teams
                .AsEnumerable() // Pull evaluation into memory to safely use GroupBy/ValueTuple
                .GroupBy(t => new ValueTuple<string, int>(t.TriCode, t.SeasonId))
                .ToDictionary(g => g.Key, g => g.First().TeamId);

            var rawRecords = await _db.RawApiResponses.Where(r => r.Endpoint == "roster").ToListAsync();

            await ProcessBatchAsync(rawRecords, "TransformRosters", async raw =>
            {
                // Skip if the EntityId is not in the expected format ("roster-TEAMCODE-SEASONID")
                string[] keyParts = raw.EntityId.Split('-');
                if (keyParts.Length < 3)
                {
                    _logger.LogError("TransformRosters skipping: Invalid EntityId format for team roster response: '{EntityId}'.", raw.EntityId);
                    return false;
                }

                string teamTriCode = keyParts[keyParts.Length - 2].Trim().ToUpper();
                if (!int.TryParse(keyParts[keyParts.Length - 1], out int seasonId))
                {
                    _logger.LogError("TransformRosters skipping: Failed to parse season ID from EntityId format: '{EntityId}'.", raw.EntityId);
                    return false;
                }

                // Ensure the team and season exist in our database core tables before processing the roster
                if (!teamLookup.TryGetValue((teamTriCode, seasonId), out int teamId) || !validSeasonIds.Contains(seasonId))
                {
                    return false;
                }

                // Automatically deserialize the entire nested structure using the DTO
                if (!TryDeserializeRawResponse<NhlRosterRootDTO>(raw, "TransformRosters", out var rosterData) || rosterData == null)
                {
                    return false;
                }

                // Flatten the three positional lists into a single collection for processing
                IEnumerable<NhlPlayerDTO> allPlayers = (rosterData.Forwards ?? Enumerable.Empty<NhlPlayerDTO>())
                .Concat(rosterData.Defensemen ?? Enumerable.Empty<NhlPlayerDTO>())
                .Concat(rosterData.Goalies ?? Enumerable.Empty<NhlPlayerDTO>());

                bool itemChanges = false;
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
                    itemChanges = true;
                }
                return itemChanges;
            });
        }

        #endregion

        #region TRANSFORM PROCESSING ENGINES

        /// <summary>
        /// Main batch execution engine handling transactional persistence and memory cleanup.
        /// </summary>
        /// <remarks>
        /// <para><strong>Purpose:</strong> The foundational low-level engine. Use directly for complex per-record transformations that don't fit a standard collection pattern.</para>
        /// <para><strong>Behavior:</strong> Iterates over items, executes the async handler, saves database changes incrementally, and clears the EF change tracker to prevent memory bloat.</para>
        /// </remarks>
        private async Task ProcessBatchAsync<T>(IEnumerable<T> items, string endpointName, Func<T, Task<bool>> processItemAsync)
        {
            foreach (var item in items)
            {
                bool hasChanges = false;
                try
                {
                    hasChanges = await processItemAsync(item);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to transform {Endpoint} payload.", endpointName);
                }

                if (hasChanges)
                {
                    await _db.SaveChangesAsync();
                    _db.ChangeTracker.Clear();
                }
            }
        }

        /// <summary>
        /// Intermediate engine for processing unique root payloads that contain nested items.
        /// </summary>
        /// <remarks>
        /// <para><strong>Purpose:</strong> Use when loading a staging endpoint whose root JSON requires global asynchronous processing per record.</para>
        /// <para><strong>Behavior:</strong> Fetches raw records, handles root-level deserialization, and delegates item execution down to Motor 1.</para>
        /// </remarks>
        private async Task ProcessEndpointAsync<TRoot>(string endpoint, string operationName, Func<TRoot, Task<bool>> processRootAsync)
            where TRoot : class
        {
            var raws = await _db.RawApiResponses.Where(r => r.Endpoint == endpoint).ToListAsync();

            await ProcessBatchAsync(raws, operationName, async raw =>
            {
                if (!TryDeserializeRawResponse<TRoot>(raw, operationName, out var root) || root == null)
                {
                    return false;
                }

                return await processRootAsync(root);
            });
        }

        /// <summary>
        /// Specialized engine to iterate through and process a collection of child items extracted from a root payload.
        /// </summary>
        /// <remarks>
        /// <para><strong>Purpose:</strong> Use when each raw response contains an array or list of items that need to be iterated over (e.g., teams or roster items).</para>
        /// <para><strong>Behavior:</strong> Deserializes the root entity, extracts the inner collection via the provided selector, and evaluates the processing logic on each item.</para>
        /// </remarks>
        private async Task ProcessEndpointCollectionAsync<TRoot, TItem>(string endpoint, string operationName, Func<TRoot, IEnumerable<TItem>> itemsSelector, Func<TItem, Task<bool>> processItemAsync)
            where TRoot : class
        {
            var raws = await _db.RawApiResponses.Where(r => r.Endpoint == endpoint).ToListAsync();

            await ProcessBatchAsync(raws, operationName, async raw =>
            {
                if (!TryDeserializeRawResponse<TRoot>(raw, operationName, out var root) || root == null)
                {
                    return false;
                }

                bool itemChanges = false;

                foreach (var item in itemsSelector(root))
                {
                    if (await processItemAsync(item))
                    {
                        itemChanges = true;
                    }
                }

                return itemChanges;
            });
        }

        /// <summary>
        /// High-level turn-key engine to automatically transform and insert standardized entities with primary key duplicate checking.
        /// </summary>
        /// <remarks>
        /// <para><strong>Purpose:</strong> The default choice for 90% of simple entities that extract items from JSON and map directly into a table.</para>
        /// <para><strong>Behavior:</strong> Automatically caches existing database keys to prevent primary key collisions, extracts items, maps them to the target entity, and stages them for insertion.</para>
        /// </remarks>
        private async Task ProcessGenericCollectionAsync<TRoot, TItem, TKey, TEntity>(string endpoint, string operationName, Func<TRoot, IEnumerable<TItem>> itemsSelector, Func<TItem, TKey> keySelector, Func<TItem, TEntity> mapEntity, DbSet<TEntity> dbSet)
            where TRoot : class
            where TEntity : class
        {
            var primaryKeyName = _db.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties[0].Name;
            var knownKeys = primaryKeyName != null
                ? await GetKnownKeysAsync<TEntity, TKey>(e => EF.Property<TKey>(e, primaryKeyName))
                : new HashSet<TKey>();

            await ProcessEndpointCollectionAsync<TRoot, TItem>(
                endpoint,
                operationName,
                itemsSelector,
                item =>
                {
                    var key = keySelector(item);
                    if (!knownKeys.Add(key))
                        return Task.FromResult(false);

                    var entity = mapEntity(item);
                    dbSet.Add(entity);
                    return Task.FromResult(true); // Ici on garde Task.FromResult car processItemAsync attend une Task<bool>
                });
        }

        #endregion

        #region HELPER METHODS

        /// <summary>
        /// Builds a mapping of team tri-codes to their corresponding list of season IDs by processing the "roster-seasons" raw API responses from the database.
        /// </summary>
        private async Task<Dictionary<string, List<int>>> BuildTricodeToSeasonsMappingAsync()
        {
            var rosterSeasonsRaws = await _db.RawApiResponses.Where(r => r.Endpoint == "roster-seasons").ToListAsync();
            var tricodeToSeasons = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

            foreach (var rRaw in rosterSeasonsRaws)
            {
                var parts = rRaw.EntityId.Split('-');
                if (parts.Length < 3)
                {
                    _logger.LogError("TransformTeams aborted: Invalid EntityId format for roster-seasons response: '{EntityId}'.", rRaw.EntityId);
                    continue;
                }
                string triCode = parts.Last();

                if (!TryDeserializeRawResponse<List<int>>(rRaw, "TransformTeams (roster-seasons)", out var seasonsList) || seasonsList == null)
                {
                    continue;
                }
                tricodeToSeasons[triCode] = seasonsList;
            }

            return tricodeToSeasons;
        }

        /// <summary>
        /// Parses a season ID into its corresponding start and end years. The season ID is expected to be in the format YYYYYYYY, where the first four digits represent the start year and the last four digits represent the end year. Falls back to a simple division and modulus operation, if the format is not as expected.
        /// </summary>
        /// <param name="seasonId"></param>
        /// <returns></returns>
        private static (int StartYear, int EndYear) ParseSeasonYears(int seasonId)
        {
            string seasonStr = seasonId.ToString();
            if (seasonStr.Length == 8 &&
                int.TryParse(seasonStr.AsSpan(0, 4), out int startYear) &&
                int.TryParse(seasonStr.AsSpan(4, 4), out int endYear))
            {
                return (startYear, endYear);
            }

            return (seasonId / 10000, seasonId % 10000);
        }

        /// <summary>
        /// Attempts to deserialize a raw API response's JSON payload into a specified type, logging errors if deserialization fails or if the payload is empty.
        /// </summary>
        private bool TryDeserializeRawResponse<T>(RawApiResponse raw, string operationName, out T? result)
            where T : class
        {
            result = default;

            if (string.IsNullOrWhiteSpace(raw.ResponseJson))
            {
                _logger.LogError("{Operation} skipping: ResponseJson is null or whitespace for EntityId '{EntityId}'.", operationName, raw.EntityId);
                return false;
            }

            if (!raw.ResponseJson.TryDeserializeSafe<T>(_logger, out result, $"{operationName.ToLower()} payload {raw.EntityId}", out _) || result == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Fetches known keys from the database for a given entity type and selector expression, returning them as a HashSet for efficient lookups.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        private async Task<HashSet<TKey>> GetKnownKeysAsync<TEntity, TKey>(Expression<Func<TEntity, TKey>> selector)
            where TEntity : class
        {
            return (await _db.Set<TEntity>().Select(selector).ToListAsync()).ToHashSet();
        }

        #endregion
    }
}