using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NHLApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditTableName_TeamRosters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamRoster");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RawApiResponse",
                table: "RawApiResponse");

            migrationBuilder.RenameTable(
                name: "RawApiResponse",
                newName: "RawApiResponses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RawApiResponses",
                table: "RawApiResponses",
                column: "Id");

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
                        name: "FK_TeamRosters_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "SeasonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamRosters_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamRosters_PlayerId",
                table: "TeamRosters",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRosters_SeasonId",
                table: "TeamRosters",
                column: "SeasonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamRosters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RawApiResponses",
                table: "RawApiResponses");

            migrationBuilder.RenameTable(
                name: "RawApiResponses",
                newName: "RawApiResponse");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RawApiResponse",
                table: "RawApiResponse",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "TeamRoster",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamRoster", x => new { x.TeamId, x.PlayerId, x.SeasonId });
                    table.ForeignKey(
                        name: "FK_TeamRoster_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamRoster_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "SeasonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamRoster_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamRoster_PlayerId",
                table: "TeamRoster",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRoster_SeasonId",
                table: "TeamRoster",
                column: "SeasonId");
        }
    }
}
