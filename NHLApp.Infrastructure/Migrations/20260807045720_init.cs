using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NHLApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
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
                name: "IX_PlayerAwards_SeasonId",
                table: "PlayerAwards",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAwards_TrophyId",
                table: "PlayerAwards",
                column: "TrophyId");

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
                name: "PlayerAwards");

            migrationBuilder.DropTable(
                name: "RawApiResponses");

            migrationBuilder.DropTable(
                name: "SeasonTotals");

            migrationBuilder.DropTable(
                name: "TeamRosters");

            migrationBuilder.DropTable(
                name: "Trophies");

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
