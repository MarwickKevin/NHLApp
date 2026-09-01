using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NHLApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FIX_NULLABLE_PROPERTIES : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscreteClip",
                table: "GamePlays");

            migrationBuilder.DropColumn(
                name: "DiscreteClipFr",
                table: "GamePlays");

            migrationBuilder.DropColumn(
                name: "HighlightClip",
                table: "GamePlays");

            migrationBuilder.DropColumn(
                name: "HighlightClipFr",
                table: "GamePlays");

            migrationBuilder.DropColumn(
                name: "HighlightClipSharingUrl",
                table: "GamePlays");

            migrationBuilder.DropColumn(
                name: "HighlightClipSharingUrlFr",
                table: "GamePlays");

            migrationBuilder.AlterColumn<string>(
                name: "VenueDefault",
                table: "Games",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "StartTimeUTC",
                table: "Games",
                type: "nvarchar(35)",
                maxLength: 35,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(35)",
                oldMaxLength: 35);

            migrationBuilder.AlterColumn<string>(
                name: "LastPeriodType",
                table: "Games",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "GameState",
                table: "Games",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "GameScheduleState",
                table: "Games",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "GameDate",
                table: "Games",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "TypeDescKey",
                table: "GamePlays",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "TimeRemaining",
                table: "GamePlays",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "TimeInPeriod",
                table: "GamePlays",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SituationCode",
                table: "GamePlays",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "PeriodType",
                table: "GamePlays",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VenueDefault",
                table: "Games",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StartTimeUTC",
                table: "Games",
                type: "nvarchar(35)",
                maxLength: 35,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(35)",
                oldMaxLength: 35,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastPeriodType",
                table: "Games",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GameState",
                table: "Games",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GameScheduleState",
                table: "Games",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GameDate",
                table: "Games",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeDescKey",
                table: "GamePlays",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TimeRemaining",
                table: "GamePlays",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TimeInPeriod",
                table: "GamePlays",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SituationCode",
                table: "GamePlays",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PeriodType",
                table: "GamePlays",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<float>(
                name: "DiscreteClip",
                table: "GamePlays",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "DiscreteClipFr",
                table: "GamePlays",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "HighlightClip",
                table: "GamePlays",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "HighlightClipFr",
                table: "GamePlays",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HighlightClipSharingUrl",
                table: "GamePlays",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HighlightClipSharingUrlFr",
                table: "GamePlays",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
