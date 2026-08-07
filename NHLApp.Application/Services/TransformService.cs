using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using NHLApp.Application.Contexts;
using NHLApp.Application.DTOs;
using NHLApp.Application.Extensions;
using NHLApp.Domain.Entities;
using NHLApp.Domain.Interfaces;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.Intrinsics.X86;

namespace NHLApp.Application.Services
{
    // TODO: Implement an IsProcessed flag on RawApiResponses to skip already-transformed staging records.
    // TODO: Make error handling consistent across all methods, including logging and exception throwing.
    // TODO: Improve commenting and documentation for each method, including parameter descriptions and return values.
    // TODO: Implement an Upsert mechanism (or update logic) to refresh existing entities if source JSON data or schema columns change.
    // TODO: Add counters of success (x/y) and show in console

    public class TransformService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransformService> _logger;

        public TransformService(IUnitOfWork unitOfWork, ILogger<TransformService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region RAW API RESPONSE TRANSFORMATION

        /// <summary>
        /// Transforms raw season JSON payloads into structured Season entities in the database, extracting start and end years from the season ID.
        /// </summary>
        /// <returns></returns>
        public async Task TransformSeasonsAsync(WorkerContext context)
        {
            await ProcessGenericCollectionAsync<List<int>, int, int, Season>(
                context,
                endpoint: "season",
                operationName: "TransformSeasons",
                itemsSelector: seasonIds => seasonIds,
                keySelector: seasonId => seasonId,
                mapEntity: seasonId =>
                {
                    var (startYear, endYear) = ParseSeasonYears(seasonId);
                    return new Season { SeasonId = seasonId, StartYear = startYear, EndYear = endYear };
                },
                dbSet: _unitOfWork.Seasons);
        }

        /// <summary>
        /// Transforms raw team JSON payloads into structured Franchise and Team entities in the database, linking teams to their franchises and seasons.
        /// </summary>
        /// <returns></returns>        
        public async Task TransformTeamsAsync(WorkerContext context)
        {
            // Build the mapping of team tri-codes to their corresponding list of season IDs using a helper
            var tricodeToSeasons = await BuildTricodeToSeasonsMappingAsync(context);

            // Load existing caches in memory
            HashSet<int> knownFranchiseIds = await GetKnownKeysAsync<Franchise, int>(f => f.FranchiseId);
            HashSet<int> knownSeasonIds = await GetKnownKeysAsync<Season, int>(s => s.SeasonId);
            HashSet<(int TeamId, int SeasonId)> knownTeamSeasons = await GetKnownKeysAsync<Team, (int TeamId, int SeasonId)>(t => ValueTuple.Create(t.TeamId, t.SeasonId));

            // Process each team item from the raw API responses, adding new franchises, seasons, and team-season relationships as needed
            await ProcessEndpointCollectionAsync<NhlTeamRootDTO, NhlTeamItemDTO>(
                context,
                endpoint: "team",
                operationName: "TransformTeams",
                itemsSelector: root => root.Data ?? Enumerable.Empty<NhlTeamItemDTO>(),
                processItemAsync: teamDto =>
                {
                    bool itemChanges = false;

                    // Add new franchises if they don't exist
                    if (teamDto.FranchiseId.HasValue && knownFranchiseIds.Add(teamDto.FranchiseId.Value))
                    {
                        _unitOfWork.Franchises.Add(new Franchise { FranchiseId = teamDto.FranchiseId.Value, Name = teamDto.FullName });
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
                                _unitOfWork.Seasons.Add(new Season { SeasonId = seasonId, StartYear = startYear, EndYear = endYear });
                                knownSeasonIds.Add(seasonId);
                                itemChanges = true;
                            }

                            // Add new team-season relationships if they don't exist
                            if (knownTeamSeasons.Contains((teamDto.Id, seasonId)))
                                continue;

                            _unitOfWork.Teams.Add(new Team
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
        /// Transforms raw player JSON payloads into basic Player entities in the database, extracting only their full name and PlayerId.
        /// </summary>
        /// <returns></returns>
        public async Task TransformPlayersAsync(WorkerContext context)
        {
            await ProcessGenericCollectionAsync<NhlRosterRootDTO, NhlPlayerDTO, int, Player>(
                context,
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
                    LastName = playerDto.LastName?.Default ?? string.Empty
                },
                dbSet: _unitOfWork.Players);
        }

        /// <summary>
        /// Transforms raw roster JSON payloads into structured TeamRosters relationships in the database, linking players to teams for specific seasons.
        /// </summary>
        /// <returns></returns>
        public async Task TransformRostersAsync(WorkerContext context)
        {
            // Load existing TeamRoster relationships into memory to avoid duplicates and round-trips            
            HashSet<(int TeamId, int PlayerId, int SeasonId)> existingRosters = await GetKnownKeysAsync<TeamRosters, (int TeamId, int PlayerId, int SeasonId)>(tr => ValueTuple.Create(tr.TeamId, tr.PlayerId, tr.SeasonId));
            HashSet<int> validPlayerIds = await GetKnownKeysAsync<Player, int>(p => p.PlayerId);
            HashSet<int> validSeasonIds = await GetKnownKeysAsync<Season, int>(s => s.SeasonId);

            // Create a lookup dictionary for team tri-codes and season IDs to their corresponding TeamId for quick access
            Dictionary<(string TriCode, int SeasonId), int> teamLookup = _unitOfWork.Teams
                .AsEnumerable() // Pull evaluation into memory to safely use GroupBy/ValueTuple
                .GroupBy(t => new ValueTuple<string, int>(t.TriCode, t.SeasonId))
                .ToDictionary(g => g.Key, g => g.First().TeamId);

            var rawRecords = await _unitOfWork.RawApiResponses.Where(r => r.Endpoint == "roster").ToListAsync();


            await ProcessBatchAsync(context, rawRecords, "TransformRosters", raw =>
            {
                // Skip if the EntityId is not in the expected format ("roster-TEAMCODE-SEASONID")
                string[] keyParts = raw.EntityId.Split('-');
                if (keyParts.Length < 3)
                {
                    context.TotalTransformErrors++;
                    _logger.LogErrorWithColor(
                        "TransformRosters skipping: Invalid EntityId format for team roster response: {EntityId}. Total transform errors: {TotalErrors}",
                        ConsoleColor.Red,
                        raw.EntityId,
                        context.TotalTransformErrors);
                    return Task.FromResult(false);
                }

                string teamTriCode = keyParts[keyParts.Length - 2].Trim().ToUpper();
                if (!int.TryParse(keyParts[keyParts.Length - 1], out int seasonId))
                {
                    context.TotalTransformErrors++;
                    _logger.LogErrorWithColor(
                        "TransformRosters skipping: Failed to parse season ID from EntityId format: {EntityId}. Total transform errors: {TotalErrors}",
                        ConsoleColor.Red,
                        raw.EntityId,
                        context.TotalTransformErrors);
                    return Task.FromResult(false);
                }

                // Ensure the team and season exist in our database core tables before processing the roster
                if (!teamLookup.TryGetValue((teamTriCode, seasonId), out int teamId) || !validSeasonIds.Contains(seasonId))
                {
                    context.TotalTransformErrors++;
                    _logger.LogErrorWithColor(
                        "TransformRosters skipping: Team or season not found for EntityId: {EntityId}. Total transform errors: {TotalErrors}",
                        ConsoleColor.Red,
                        raw.EntityId,
                        context.TotalTransformErrors);
                    return Task.FromResult(false);
                }

                // Automatically deserialize the entire nested structure using the DTO
                if (!TryDeserializeRawResponse<NhlRosterRootDTO>(raw, "TransformRosters", out var rosterData) || rosterData == null)
                {
                    context.TotalTransformErrors++;
                    _logger.LogErrorWithColor(
                        "TransformRosters skipping: Failed to deserialize roster data for EntityId: {EntityId}. Total transform errors: {TotalErrors}",
                        ConsoleColor.Red,
                        raw.EntityId,
                        context.TotalTransformErrors);
                    return Task.FromResult(false);
                }

                // Flatten the three positional lists into a single collection for processing
                IEnumerable<NhlPlayerDTO> allPlayers = (rosterData.Forwards ?? Enumerable.Empty<NhlPlayerDTO>())
                .Concat(rosterData.Defensemen ?? Enumerable.Empty<NhlPlayerDTO>())
                .Concat(rosterData.Goalies ?? Enumerable.Empty<NhlPlayerDTO>());

                bool itemChanges = false;
                // Loop through each player DTO and add new TeamRoster relationships to the TeamRosters table if they don't already exist
                foreach (NhlPlayerDTO playerDto in allPlayers)
                {
                    if (!validPlayerIds.Contains(playerDto.Id))
                    {
                        context.TotalTransformErrors++;
                        _logger.LogErrorWithColor(
                            "TransformRosters skipping: Player ID {PlayerId} not found in valid players for team roster (TeamId: {TeamId}, SeasonId: {SeasonId}). Total transform errors: {TotalErrors}",
                            ConsoleColor.Red,
                            playerDto.Id,
                            teamId,
                            seasonId,
                            context.TotalTransformErrors);
                        continue;
                    }

                    if (existingRosters.Contains((teamId, playerDto.Id, seasonId)))
                    {
                        _logger.LogDebug("Transform Roster skipping: Relationship already exists for PlayerId {PlayerId}, TeamId {TeamId} and SeasonId {SeasonId}",
                            playerDto.Id,
                            teamId,
                            seasonId);
                        continue;
                    }

                    _unitOfWork.TeamRosters.Add(new TeamRosters
                    {
                        TeamId = teamId,
                        PlayerId = playerDto.Id,
                        SeasonId = seasonId
                    });
                    existingRosters.Add((teamId, playerDto.Id, seasonId));
                    itemChanges = true;
                }
                return Task.FromResult(itemChanges);
            });
        }

        /// <summary>
        /// Transforms raw player landing JSON payloads into structured complex entities (Drafts, Season Totals, etc.) in the database.
        /// </summary>
        /// <returns></returns>
        public async Task TransformPlayerLandingsAsync(WorkerContext context)
        {
            HashSet<int> validPlayerIds = await GetKnownKeysAsync<Player, int>(p => p.PlayerId);

            await ProcessEndpointAsync<NhlPlayerLandingDTO>(
                context,
                endpoint: "player-landing",
                operationName: "TransformPlayerLandings",
                processRootAsync: landing =>
                {
                    bool itemChanges = false;
                    var player = _unitOfWork.Players
                        .Include(p => p.DraftDetail)
                        .Include(p => p.SeasonTotal)
                        .Include(p => p.PlayerAwards)
                        .FirstOrDefault(p => p.PlayerId == landing.PlayerId);


                    if (player != null)
                    {
                        player.Headshot = landing.Headshot;
                        player.SweaterNumber = landing.SweaterNumber;
                        player.Position = landing.Position;
                        player.ShootsCatches = landing.ShootsCatches;

                        DateOnly? birthDate = null;
                        if (!string.IsNullOrEmpty(landing.BirthDate) && DateOnly.TryParse(landing.BirthDate, out var parsedDate))
                        {
                            birthDate = parsedDate;
                        }

                        player.BirthDate = birthDate;
                        player.BirthCity = landing.BirthCity?.Default;
                        player.BirthStateProvince = landing.BirthStateProvince?.Default;
                        player.BirthCountry = landing.BirthCountry;

                        player.HeightInCentimeters = landing.HeightInCentimeters;
                        player.HeightInInches = landing.HeightInInches;
                        player.WeightInKilograms = landing.WeightInKilograms;
                        player.WeightInPounds = landing.WeightInPounds;

                        player.IsActive = landing.IsActive;
                        player.CurrentTeamId = landing.CurrentTeamId;
                        player.CurrentTeamAbbrev = landing.CurrentTeamAbbrev;
                        player.FullTeamName = landing.FullTeamName?.Default;
                        player.TeamCommonName = landing.TeamCommonName?.Default;
                        player.TeamPlaceNameWithPreposition = landing.TeamPlaceNameWithPreposition?.Default;
                        player.TeamLogo = landing.TeamLogo;

                        player.HeroImage = landing.HeroImage;
                        player.PlayerSlug = landing.PlayerSlug;

                        // 1. Map Draft Details (1-to-0..1 relationship)
                        if (landing.DraftDetails != null && player.DraftDetail == null)
                        {
                            player.DraftDetail = new DraftDetail
                            {
                                PlayerId = landing.PlayerId,

                                Year = landing.DraftDetails.Year,
                                TeamAbbrev = landing.DraftDetails.TeamAbbrev,
                                Round = landing.DraftDetails.Round,
                                PickInRound = landing.DraftDetails.PickInRound,
                                OverallPick = landing.DraftDetails.OverallPick
                            };
                        }

                        // 2. Map Season Totals (1-to-Many relationship)
                        if (landing.SeasonTotals != null && landing.SeasonTotals.Any())
                        {
                            foreach (var seasonDto in landing.SeasonTotals)
                            {
                                bool exists = player.SeasonTotal.Any(st => st.Season == seasonDto.Season && st.Sequence == seasonDto.Sequence && st.GameTypeId == seasonDto.GameTypeId);
                                if (exists) continue;

                                var seasonTotalEntity = new SeasonTotal
                                {
                                    PlayerId = landing.PlayerId,

                                    Assists = seasonDto.Assists,
                                    GameTypeId = seasonDto.GameTypeId,
                                    GamesPlayed = seasonDto.GamesPlayed,
                                    Goals = seasonDto.Goals,
                                    LeagueAbbrev = seasonDto.LeagueAbbrev,
                                    Pim = seasonDto.Pim,
                                    Points = seasonDto.Points,
                                    Season = seasonDto.Season,
                                    Sequence = seasonDto.Sequence,
                                    TeamName = seasonDto.TeamName?.Default,
                                    GameWinningGoals = seasonDto.GameWinningGoals,
                                    PlusMinus = seasonDto.PlusMinus,
                                    PowerPlayGoals = seasonDto.PowerPlayGoals,
                                    ShorthandedGoals = seasonDto.ShorthandedGoals,
                                    Shots = seasonDto.Shots,
                                    TeamCommonName = seasonDto.TeamCommonName?.Default,
                                    TeamPlaceNameWithPreposition = seasonDto.TeamPlaceNameWithPreposition?.Default,
                                    AvgToi = seasonDto.AvgToi ?? string.Empty,
                                    FaceoffWinningPctg = seasonDto.FaceoffWinningPctg,
                                    OtGoals = seasonDto.OtGoals,
                                    PowerPlayPoints = seasonDto.PowerPlayPoints,
                                    ShootingPctg = seasonDto.ShootingPctg,
                                    ShorthandedPoints = seasonDto.ShorthandedPoints
                                };

                                player.SeasonTotal.Add(seasonTotalEntity);
                            }
                        }

                        // 3. Map Awards (1-to-Many relationship)
                        if (landing.Awards != null && landing.Awards.Any())
                        {
                            foreach (var awardDto in landing.Awards)
                            {
                                if (awardDto.Seasons == null) continue;

                                string trophyName = awardDto.Trophy?.Default ?? string.Empty;
                                if (string.IsNullOrWhiteSpace(trophyName)) continue;

                                var trophy = _unitOfWork.Trophies.Local.FirstOrDefault(t => t.Name.Equals(trophyName, StringComparison.OrdinalIgnoreCase))
                                             ?? _unitOfWork.Trophies.FirstOrDefault(t => t.Name == trophyName);

                                if (trophy == null)
                                {
                                    trophy = new Trophy { Name = trophyName };
                                    _unitOfWork.Trophies.Add(trophy);
                                }

                                foreach (var seasonDto in awardDto.Seasons)
                                {
                                    bool exists = player.PlayerAwards.Any(pa => pa.SeasonId == seasonDto.SeasonId && pa.TrophyId == trophy.Id);
                                    if (exists) continue;

                                    var awardEntity = new PlayerAwards
                                    {
                                        PlayerId = landing.PlayerId,
                                        SeasonId = seasonDto.SeasonId,
                                        Trophy = trophy
                                    };

                                    player.PlayerAwards.Add(awardEntity);
                                }
                            }
                        }

                        itemChanges = true;
                    }

                    return Task.FromResult(itemChanges);
                });

        }

        /// <summary>
        /// Transforms raw weekly schedule JSON payloads into basic Game entities containing only their unique Ids.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task TransformWeeklySchedulesAsync(WorkerContext context)
        {
            await ProcessGenericCollectionAsync<ScheduleRootDTO, GameDTO, long, Game>(
                context,
                endpoint: "schedule",
                operationName: "TransformWeeklySchedule",
                itemsSelector: root =>
                    root.GameWeek?
                        .SelectMany(week => week.Games ?? Enumerable.Empty<GameDTO>())
                    ?? Enumerable.Empty<GameDTO>(),
                keySelector: gameDto => gameDto.Id,
                mapEntity: gameDto => new Game
                {
                    Id = gameDto.Id
                },
                dbSet: _db.Games);
        }

        #endregion

        #region TRANSFORM PROCESSING ENGINES

        //////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////     Level 1      ////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////

        //------------------------------------------------------------------------------------------------------------------------
        // Use directly only when a transformation requires custom per-item processing that does not fit the higher-level engines.

        /// <summary>
        /// Level 1: Low-level batch engine responsible for executing transformations on individual items.
        /// </summary>
        /// <typeparam name="T">The type of item being processed.</typeparam>
        /// <param name="context">The worker context tracking processing state and transformation errors.</param>
        /// <param name="items">The collection of items to process.</param>
        /// <param name="operationName">The operation name used for logging.</param>
        /// <param name="processItemAsync">The asynchronous transformation function executed for each item. Returns true when database changes were made.</param>
        /// <remarks>
        /// <para><strong>Purpose:</strong> Provides the foundation for all transformation workflows by centralizing item execution, persistence, error handling, and EF Core memory cleanup.</para>
        /// <para><strong>When to use:</strong> Use directly only when a transformation requires custom per-item processing that does not fit the higher-level engines.</para>
        /// <para><strong>Behavior:</strong> Executes each item handler individually, saves changes when required, logs transformation failures, and clears the EF Core change tracker after each item.</para>
        /// </remarks>
        private async Task ProcessBatchAsync<T>(WorkerContext context, IEnumerable<T> items, string operationName, Func<T, Task<bool>> processItemAsync)
        {
            foreach (var item in items)
            {
                bool hasChanges = false;
                try
                {
                    hasChanges = await processItemAsync(item);

                    if (hasChanges)
                    {
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    context.TotalTransformErrors++;
                    _logger.LogErrorWithColor(
                        ex,
                        "Failed to transform '{operationName}' payload. Total transform errors: {TotalErrors}",
                        ConsoleColor.Red,
                        operationName,
                        context.TotalTransformErrors);
                }
                finally
                {
                    _unitOfWork.ChangeTracker.Clear();
                }
            }
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////     Level 2     ////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////

        //---------------------------------------------------------------------------------------------------------------
        // Use when the transformation requires access to the complete JSON payload instead of individual extracted items:

        /// <summary>
        /// Level 2: Intermediate engine for processing raw API responses where the complete root DTO requires custom handling.
        /// </summary>
        /// <typeparam name="TRoot">The root DTO type representing the deserialized JSON structure.</typeparam>
        /// <param name="context">The worker context tracking processing state and transformation errors.</param>
        /// <param name="endpoint">The raw API response endpoint to query.</param>
        /// <param name="operationName">The operation name used for logging and tracking.</param>
        /// <param name="processRootAsync">The asynchronous transformation function executed using the deserialized root DTO.</param>
        /// <remarks>
        /// <para><strong>Purpose:</strong> Provides the common workflow for loading raw responses and deserializing root DTOs while allowing custom root-level transformation logic.</para>
        /// <para><strong>When to use:</strong> Use when the transformation requires access to the complete JSON payload instead of individual extracted items.</para>
        /// <para><strong>Behavior:</strong> Loads raw responses, deserializes each payload into the root DTO, and delegates processing to the provided handler.</para>
        /// </remarks>

        private async Task ProcessEndpointAsync<TRoot>(WorkerContext context, string endpoint, string operationName, Func<TRoot, Task<bool>> processRootAsync)
            where TRoot : class
        {
            var raws = await _unitOfWork.RawApiResponses.Where(r => r.Endpoint == endpoint).ToListAsync();

            await ProcessBatchAsync(context, raws, operationName, async raw =>
            {
                // If the root deserialization fails, skip processing this record
                if (!TryDeserializeRawResponse<TRoot>(raw, operationName, out var root) || root == null)
                {
                    _logger.LogErrorWithColor(
                        "{OperationName} skipping: Failed to deserialize root payload for EntityId '{EntityId}'. Total transform errors: {TotalErrors}",
                        ConsoleColor.Red,
                        operationName,
                        raw.EntityId,
                        context.TotalTransformErrors);
                    return false;
                }

                return await processRootAsync(root);
            });
        }
        

        //--------------------------------------------------------------------------------
        // Use when each extracted item requires custom processing before being persisted:

        /// <summary>
        /// Level 2: Intermediate engine for processing collections of child items extracted from root DTOs.
        /// </summary>
        /// <typeparam name="TRoot">The root DTO type representing the deserialized JSON structure.</typeparam>
        /// <typeparam name="TItem">The individual item type extracted from the root DTO.</typeparam>
        /// <param name="context">The worker context tracking processing state and transformation errors.</param>
        /// <param name="endpoint">The raw API response endpoint to query.</param>
        /// <param name="operationName">The operation name used for logging and tracking.</param>
        /// <param name="itemsSelector">Function that extracts the item collection from the root DTO.</param>
        /// <param name="processItemAsync">The asynchronous transformation function executed for each extracted item.</param>
        /// <remarks>
        /// <para><strong>Purpose:</strong> Provides a reusable workflow for processing JSON payloads containing collections of child items requiring custom transformation logic.</para>
        /// <para><strong>When to use:</strong> Use when each extracted item requires custom processing before being persisted.</para>
        /// <para><strong>Behavior:</strong> Deserializes root DTOs, extracts items using the selector, processes each item, and delegates persistence handling to the batch engine.</para>
        /// </remarks>
        private async Task ProcessEndpointCollectionAsync<TRoot, TItem>(WorkerContext context, string endpoint, string operationName, Func<TRoot, IEnumerable<TItem>> itemsSelector, Func<TItem, Task<bool>> processItemAsync)
            where TRoot : class
        {
            var raws = await _unitOfWork.RawApiResponses.Where(r => r.Endpoint == endpoint).ToListAsync();

            /// Use the batch processing engine to handle each raw response, deserialize the root payload, and process each item in the extracted collection
            await ProcessBatchAsync(context, raws, operationName, async raw =>
            {
                // Attempt to deserialize the root payload from the raw response. If deserialization fails, skip processing this record.
                if (!TryDeserializeRawResponse<TRoot>(raw, operationName, out var root) || root == null)
                {
                    context.TotalTransformErrors++;
                    _logger.LogErrorWithColor(
                        "{OperationName} skipping: Failed to deserialize root payload for EntityId '{EntityId}'. Total transform errors: {TotalErrors}",
                        ConsoleColor.Red,
                        operationName,
                        raw.EntityId,
                        context.TotalTransformErrors);
                    return false;
                }

                bool itemChanges = false;

                // Loop through each item extracted from the root payload and process it using the provided async handler
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



        /////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////     Level 3     ////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////


        //----------------------------------------------------------------------------------------------------------
        // Use when extracted items can be mapped directly into entities without additional custom processing logic:

        /// <summary>
        /// Level 3: High-level generic engine for transforming API collections into EF Core entities.
        /// </summary>
        /// <typeparam name="TRoot">The root DTO type representing the JSON response structure.</typeparam>
        /// <typeparam name="TItem">The individual item DTO type extracted from the root DTO.</typeparam>
        /// <typeparam name="TKey">The key type used for duplicate detection (int, long, Guid, etc.).</typeparam>
        /// <typeparam name="TEntity">The EF Core entity type being inserted.</typeparam>
        /// <param name="context">The worker context tracking processing state and transformation errors.</param>
        /// <param name="endpoint">The raw API response endpoint to query.</param>
        /// <param name="operationName">The operation name used for logging and tracking.</param>
        /// <param name="itemsSelector">Function that extracts the item collection from the root DTO.</param>
        /// <param name="keySelector">Function that extracts the unique key from an item DTO.</param>
        /// <param name="mapEntity">Function that converts an item DTO into an EF Core entity.</param>
        /// <param name="dbSet">The EF Core DbSet where new entities are added.</param>
        /// <remarks>
        /// <para><strong>Purpose:</strong> Provides a reusable DTO-to-entity transformation pipeline for standard API collection imports.</para>
        /// <para><strong>When to use:</strong> Use when extracted items can be mapped directly into entities without additional custom processing logic.</para>
        /// <para><strong>Behavior:</strong> Loads existing keys, skips duplicates, maps DTOs into entities, stages new entities for insertion, and delegates persistence handling to the batch engine.</para>
        /// </remarks>
        private async Task ProcessGenericCollectionAsync<TRoot, TItem, TKey, TEntity>(WorkerContext context, string endpoint, string operationName, Func<TRoot, IEnumerable<TItem>> itemsSelector, Func<TItem, TKey> keySelector, Func<TItem, TEntity> mapEntity, DbSet<TEntity> dbSet)
            where TRoot : class
            where TEntity : class
        {
            var primaryKeyName = _unitOfWork.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties[0].Name;
            var knownKeys = primaryKeyName != null
                ? await GetKnownKeysAsync<TEntity, TKey>(e => EF.Property<TKey>(e, primaryKeyName))
                : new HashSet<TKey>();

            await ProcessEndpointCollectionAsync<TRoot, TItem>(
                context,
                endpoint,
                operationName,
                itemsSelector,
                item =>
                {
                    var key = keySelector(item);
                    if (!knownKeys.Add(key))
                    {
                        _logger.LogDebug("{endpoint} skipping: Duplicate primary key {Key} for entity {EntityType}.", endpoint, key, typeof(TEntity).Name);
                        return Task.FromResult(false);
                    }

                    var entity = mapEntity(item);
                    dbSet.Add(entity);
                    return Task.FromResult(true);
                });
        }

        #endregion

        #region HELPER METHODS

        /// <summary>
        /// Builds a mapping of team tri-codes to their corresponding list of season IDs by processing the "roster-seasons" raw API responses from the database.
        /// </summary>
        private async Task<Dictionary<string, List<int>>> BuildTricodeToSeasonsMappingAsync(WorkerContext context)
        {
            var rosterSeasonsRaws = await _unitOfWork.RawApiResponses.Where(r => r.Endpoint == "roster-seasons").ToListAsync();
            var tricodeToSeasons = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

            // Loop through each raw roster-seasons response, validate the EntityId format, and deserialize the JSON payload into a list of season IDs.
            foreach (var rRaw in rosterSeasonsRaws)
            {
                var parts = rRaw.EntityId.Split('-');
                // Validate that the EntityId is in the expected format ("roster-seasons-TRICODE") and extract the tri-code. If the format is invalid, skip this record.
                if (parts.Length < 3)
                {
                    context.TotalTransformErrors++;
                    _logger.LogErrorWithColor(
                        "TransformTeams skipping: Invalid EntityId format for roster-seasons response: '{EntityId}'. Total transform errors: {TotalErrors}",
                        ConsoleColor.Red,
                        rRaw.EntityId,
                        context.TotalTransformErrors);
                    continue;
                }
                string triCode = parts.Last();

                // Attempt to deserialize the raw response JSON into a list of season IDs. If deserialization fails or the result is null, skip this record.
                if (!TryDeserializeRawResponse<List<int>>(rRaw, "TransformTeams (roster-seasons)", out var seasonsList) || seasonsList == null)
                {
                    context.TotalTransformErrors++;
                    _logger.LogErrorWithColor(
                        "TransformTeams skipping: Failed to deserialize roster-seasons for EntityId '{EntityId}'. Total transform errors: {TotalErrors}",
                        ConsoleColor.Red,
                        rRaw.EntityId,
                        context.TotalTransformErrors);
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
                _logger.LogError("Raw response JSON is null, empty, or whitespace for entity type {EntityType} with ID {EntityId}.", typeof(T).Name, raw.Id);
                return false;
            }

            if (!raw.ResponseJson.TryDeserializeSafe<T>(out result, out var parseError) || result == null)
            {
                _logger.LogError("Failed to deserialize raw response JSON for entity type {EntityType} with ID {EntityId}. Error: {ParseError}", typeof(T).Name, raw.Id, parseError);
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
            return (await _unitOfWork.Set<TEntity>().Select(selector).ToListAsync()).ToHashSet();
        }

        #endregion
    }
}