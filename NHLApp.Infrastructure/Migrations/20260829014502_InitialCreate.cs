using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NHLApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Franchises",
                columns: table => new
                {
                    FranchiseId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Franchises", x => x.FranchiseId);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Season = table.Column<int>(type: "int", nullable: false),
                    GameType = table.Column<int>(type: "int", nullable: false),
                    LimitedScoring = table.Column<bool>(type: "bit", nullable: false),
                    GameDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VenueDefault = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    VenueLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartTimeUTC = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    GameState = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GameScheduleState = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AwayTeamId = table.Column<int>(type: "int", nullable: false),
                    HomeTeamId = table.Column<int>(type: "int", nullable: false),
                    AwayScore = table.Column<int>(type: "int", nullable: false),
                    HomeScore = table.Column<int>(type: "int", nullable: false),
                    AwaySog = table.Column<int>(type: "int", nullable: false),
                    HomeSog = table.Column<int>(type: "int", nullable: false),
                    ShootoutInUse = table.Column<bool>(type: "bit", nullable: false),
                    OtInUse = table.Column<bool>(type: "bit", nullable: false),
                    LastPeriodType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Tie = table.Column<bool>(type: "bit", nullable: true),
                    OtPeriods = table.Column<int>(type: "int", nullable: true),
                    RegPeriods = table.Column<int>(type: "int", nullable: true),
                    SpecialEventParentId = table.Column<int>(type: "int", nullable: true),
                    SpecialEventName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Headshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SweaterNumber = table.Column<int>(type: "int", nullable: true),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShootsCatches = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BirthCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthStateProvince = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthCountry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeightInCentimeters = table.Column<int>(type: "int", nullable: true),
                    WeightInKilograms = table.Column<int>(type: "int", nullable: true),
                    HeightInInches = table.Column<int>(type: "int", nullable: true),
                    WeightInPounds = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    CurrentTeamId = table.Column<int>(type: "int", nullable: true),
                    CurrentTeamAbbrev = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullTeamName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamCommonName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamPlaceNameWithPreposition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamLogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeroImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlayerSlug = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "RawApiResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawApiResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    StartYear = table.Column<int>(type: "int", nullable: true),
                    EndYear = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.SeasonId);
                });

            migrationBuilder.CreateTable(
                name: "Trophies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trophies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GamePlays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    PeriodNumber = table.Column<int>(type: "int", nullable: false),
                    PeriodType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TimeInPeriod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TimeRemaining = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    TypeDescKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    SituationCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    HomeTeamDefendingSide = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    EventOwnerTeamId = table.Column<int>(type: "int", nullable: true),
                    AwayScore = table.Column<int>(type: "int", nullable: true),
                    HomeScore = table.Column<int>(type: "int", nullable: true),
                    AwaySOG = table.Column<int>(type: "int", nullable: true),
                    HomeSOG = table.Column<int>(type: "int", nullable: true),
                    ShotType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShootingPlayerId = table.Column<int>(type: "int", nullable: true),
                    ScoringPlayerId = table.Column<int>(type: "int", nullable: true),
                    ScoringPlayerTotal = table.Column<int>(type: "int", nullable: true),
                    Assist1PlayerId = table.Column<int>(type: "int", nullable: true),
                    Assist1PlayerTotal = table.Column<int>(type: "int", nullable: true),
                    Assist2PlayerId = table.Column<int>(type: "int", nullable: true),
                    Assist2PlayerTotal = table.Column<int>(type: "int", nullable: true),
                    Assist3PlayerId = table.Column<int>(type: "int", nullable: true),
                    Assist3PlayerTotal = table.Column<int>(type: "int", nullable: true),
                    GoalieInNetId = table.Column<int>(type: "int", nullable: true),
                    GoalInGame = table.Column<int>(type: "int", nullable: true),
                    HittingPlayerId = table.Column<int>(type: "int", nullable: true),
                    HitteePlayerId = table.Column<int>(type: "int", nullable: true),
                    BlockingPlayerId = table.Column<int>(type: "int", nullable: true),
                    WinningPlayerId = table.Column<int>(type: "int", nullable: true),
                    LosingPlayerId = table.Column<int>(type: "int", nullable: true),
                    CommittedByPlayerId = table.Column<int>(type: "int", nullable: true),
                    DrawnByPlayerId = table.Column<int>(type: "int", nullable: true),
                    ServedByPlayerId = table.Column<int>(type: "int", nullable: true),
                    PenaltyDuration = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SecondaryReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    XCoord = table.Column<int>(type: "int", nullable: true),
                    YCoord = table.Column<int>(type: "int", nullable: true),
                    ZoneCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    DetailsTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailsDescKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    DiscreteClip = table.Column<float>(type: "real", nullable: true),
                    DiscreteClipFr = table.Column<float>(type: "real", nullable: true),
                    HighlightClip = table.Column<float>(type: "real", nullable: true),
                    HighlightClipFr = table.Column<float>(type: "real", nullable: true),
                    HighlightClipSharingUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HighlightClipSharingUrlFr = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePlays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GamePlays_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoalieGameStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    SweaterNumber = table.Column<int>(type: "int", nullable: false),
                    Toi = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Starter = table.Column<bool>(type: "bit", nullable: false),
                    ShotsAgainst = table.Column<int>(type: "int", nullable: false),
                    Saves = table.Column<int>(type: "int", nullable: false),
                    GoalsAgainst = table.Column<int>(type: "int", nullable: false),
                    SavePctg = table.Column<float>(type: "real", nullable: false),
                    Pim = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    EvenStrengthShotsAgainst = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    PowerPlayShotsAgainst = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    ShorthandedShotsAgainst = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    SaveShotsAgainst = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    EvenStrengthGoalsAgainst = table.Column<int>(type: "int", nullable: false),
                    PowerPlayGoalsAgainst = table.Column<int>(type: "int", nullable: false),
                    ShorthandedGoalsAgainst = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalieGameStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalieGameStats_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerGameStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    SweaterNumber = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Goals = table.Column<int>(type: "int", nullable: false),
                    Assists = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    PlusMinus = table.Column<int>(type: "int", nullable: false),
                    Pim = table.Column<int>(type: "int", nullable: false),
                    Hits = table.Column<int>(type: "int", nullable: false),
                    PowerPlayGoals = table.Column<int>(type: "int", nullable: false),
                    Sog = table.Column<int>(type: "int", nullable: false),
                    FaceoffWinningPctg = table.Column<float>(type: "real", nullable: false),
                    Toi = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BlockedShots = table.Column<int>(type: "int", nullable: false),
                    Shifts = table.Column<int>(type: "int", nullable: false),
                    Giveaways = table.Column<int>(type: "int", nullable: false),
                    Takeaways = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerGameStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerGameStats_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DraftDetail",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: true),
                    TeamAbbrev = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Round = table.Column<int>(type: "int", nullable: true),
                    PickInRound = table.Column<int>(type: "int", nullable: true),
                    OverallPick = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftDetail", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_DraftDetail_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeasonTotals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Assists = table.Column<int>(type: "int", nullable: true),
                    GameTypeId = table.Column<int>(type: "int", nullable: true),
                    GamesPlayed = table.Column<int>(type: "int", nullable: true),
                    Goals = table.Column<int>(type: "int", nullable: true),
                    LeagueAbbrev = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pim = table.Column<int>(type: "int", nullable: true),
                    Points = table.Column<int>(type: "int", nullable: true),
                    Season = table.Column<int>(type: "int", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: true),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GameWinningGoals = table.Column<int>(type: "int", nullable: true),
                    PlusMinus = table.Column<int>(type: "int", nullable: true),
                    PowerPlayGoals = table.Column<int>(type: "int", nullable: true),
                    ShorthandedGoals = table.Column<int>(type: "int", nullable: true),
                    Shots = table.Column<int>(type: "int", nullable: true),
                    TeamCommonName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamPlaceNameWithPreposition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvgToi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FaceoffWinningPctg = table.Column<float>(type: "real", nullable: true),
                    OtGoals = table.Column<int>(type: "int", nullable: true),
                    PowerPlayPoints = table.Column<int>(type: "int", nullable: true),
                    ShootingPctg = table.Column<float>(type: "real", nullable: true),
                    ShorthandedPoints = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonTotals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonTotals_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    FranchiseId = table.Column<int>(type: "int", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TriCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RawTriCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeagueId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => new { x.TeamId, x.SeasonId });
                    table.ForeignKey(
                        name: "FK_Teams_Franchises_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchises",
                        principalColumn: "FranchiseId");
                    table.ForeignKey(
                        name: "FK_Teams_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "SeasonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerAwards",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TrophyId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAwards", x => new { x.PlayerId, x.TrophyId, x.SeasonId });
                    table.ForeignKey(
                        name: "FK_PlayerAwards_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerAwards_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "SeasonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerAwards_Trophies_TrophyId",
                        column: x => x.TrophyId,
                        principalTable: "Trophies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamRosters",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamRosters", x => new { x.TeamId, x.PlayerId, x.SeasonId });
                    table.ForeignKey(
                        name: "FK_TeamRosters_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamRosters_Teams_TeamId_SeasonId",
                        columns: x => new { x.TeamId, x.SeasonId },
                        principalTable: "Teams",
                        principalColumns: new[] { "TeamId", "SeasonId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GamePlays_GameId_EventId",
                table: "GamePlays",
                columns: new[] { "GameId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalieGameStats_GameId_PlayerId_TeamId",
                table: "GoalieGameStats",
                columns: new[] { "GameId", "PlayerId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAwards_SeasonId",
                table: "PlayerAwards",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAwards_TrophyId",
                table: "PlayerAwards",
                column: "TrophyId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameStats_GameId_PlayerId_TeamId",
                table: "PlayerGameStats",
                columns: new[] { "GameId", "PlayerId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeasonTotals_PlayerId",
                table: "SeasonTotals",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRosters_PlayerId",
                table: "TeamRosters",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRosters_TeamId_SeasonId",
                table: "TeamRosters",
                columns: new[] { "TeamId", "SeasonId" });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_FranchiseId",
                table: "Teams",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_SeasonId",
                table: "Teams",
                column: "SeasonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftDetail");

            migrationBuilder.DropTable(
                name: "GamePlays");

            migrationBuilder.DropTable(
                name: "GoalieGameStats");

            migrationBuilder.DropTable(
                name: "PlayerAwards");

            migrationBuilder.DropTable(
                name: "PlayerGameStats");

            migrationBuilder.DropTable(
                name: "RawApiResponses");

            migrationBuilder.DropTable(
                name: "SeasonTotals");

            migrationBuilder.DropTable(
                name: "TeamRosters");

            migrationBuilder.DropTable(
                name: "Trophies");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Franchises");

            migrationBuilder.DropTable(
                name: "Seasons");
        }
    }
}
