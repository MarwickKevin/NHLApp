using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NHLApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Players_AddProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BirthStateProvince",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Headshot",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeightInInches",
                table: "Players",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SweaterNumber",
                table: "Players",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeightInPounds",
                table: "Players",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthStateProvince",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Headshot",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "HeightInInches",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "SweaterNumber",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "WeightInPounds",
                table: "Players");
        }
    }
}
