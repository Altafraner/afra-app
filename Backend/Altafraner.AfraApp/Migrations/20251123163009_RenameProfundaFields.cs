using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class RenameProfundaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "minKlasse",
                table: "Profunda",
                newName: "MinKlasse");

            migrationBuilder.RenameColumn(
                name: "maxKlasse",
                table: "Profunda",
                newName: "MaxKlasse");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MinKlasse",
                table: "Profunda",
                newName: "minKlasse");

            migrationBuilder.RenameColumn(
                name: "MaxKlasse",
                table: "Profunda",
                newName: "maxKlasse");
        }
    }
}
