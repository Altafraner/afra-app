using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class ProfundumKlassenLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "maxKlasse",
                table: "Profunda",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "minKlasse",
                table: "Profunda",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "maxKlasse",
                table: "Profunda");

            migrationBuilder.DropColumn(
                name: "minKlasse",
                table: "Profunda");
        }
    }
}
