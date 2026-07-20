using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NHLApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeKey_SeasonToTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamRosters_Seasons_SeasonId",
                table: "TeamRosters");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamRosters_Teams_TeamId",
                table: "TeamRosters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Teams",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_TeamRosters_SeasonId",
                table: "TeamRosters");

            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "Teams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Teams",
                table: "Teams",
                columns: new[] { "TeamId", "SeasonId" });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_SeasonId",
                table: "Teams",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRosters_TeamId_SeasonId",
                table: "TeamRosters",
                columns: new[] { "TeamId", "SeasonId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TeamRosters_Teams_TeamId_SeasonId",
                table: "TeamRosters",
                columns: new[] { "TeamId", "SeasonId" },
                principalTable: "Teams",
                principalColumns: new[] { "TeamId", "SeasonId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Seasons_SeasonId",
                table: "Teams",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "SeasonId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamRosters_Teams_TeamId_SeasonId",
                table: "TeamRosters");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Seasons_SeasonId",
                table: "Teams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Teams",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_SeasonId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_TeamRosters_TeamId_SeasonId",
                table: "TeamRosters");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "Teams");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Teams",
                table: "Teams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRosters_SeasonId",
                table: "TeamRosters",
                column: "SeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamRosters_Seasons_SeasonId",
                table: "TeamRosters",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "SeasonId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamRosters_Teams_TeamId",
                table: "TeamRosters",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
