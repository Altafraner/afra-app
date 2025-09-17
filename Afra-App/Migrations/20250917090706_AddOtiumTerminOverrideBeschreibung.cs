using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class AddOtiumTerminOverrideBeschreibung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OverrideBeschreibung",
                table: "OtiaTermine",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverrideBeschreibung",
                table: "OtiaTermine");
        }
    }
}
