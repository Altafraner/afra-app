using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class RenameFirstAndLastName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                table: "Personen",
                name: "Vorname",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                table: "Personen",
                name: "Nachname",
                newName: "LastName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                table: "Personen",
                name: "LastName",
                newName: "Nachname");

            migrationBuilder.RenameColumn(
                table: "Personen",
                name: "FirstName",
                newName: "Vorname");
        }
    }
}
