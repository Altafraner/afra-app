using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class AddOtiumKlassenLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxKlasse",
                table: "Otia",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinKlasse",
                table: "Otia",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxKlasse",
                table: "Otia");

            migrationBuilder.DropColumn(
                name: "MinKlasse",
                table: "Otia");
        }
    }
}
