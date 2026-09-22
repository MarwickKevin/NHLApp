using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NHLApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_DB_Relationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Season",
                table: "Games",
                newName: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameStats_PlayerId",
                table: "PlayerGameStats",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalieGameStats_PlayerId",
                table: "GoalieGameStats",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_SeasonId",
                table: "Games",
                column: "SeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Seasons_SeasonId",
                table: "Games",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "SeasonId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoalieGameStats_Players_PlayerId",
                table: "GoalieGameStats",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerGameStats_Players_PlayerId",
                table: "PlayerGameStats",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Seasons_SeasonId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_GoalieGameStats_Players_PlayerId",
                table: "GoalieGameStats");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerGameStats_Players_PlayerId",
                table: "PlayerGameStats");

            migrationBuilder.DropIndex(
                name: "IX_PlayerGameStats_PlayerId",
                table: "PlayerGameStats");

            migrationBuilder.DropIndex(
                name: "IX_GoalieGameStats_PlayerId",
                table: "GoalieGameStats");

            migrationBuilder.DropIndex(
                name: "IX_Games_SeasonId",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "SeasonId",
                table: "Games",
                newName: "Season");
        }
    }
}
