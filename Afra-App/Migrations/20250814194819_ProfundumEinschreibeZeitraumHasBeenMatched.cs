using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class ProfundumEinschreibeZeitraumHasBeenMatched : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasBeenMatched",
                table: "ProfundumEinwahlZeitraeume",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasBeenMatched",
                table: "ProfundumEinwahlZeitraeume");
        }
    }
}
