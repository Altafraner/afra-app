using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddWantedEinschreibungenToProfundumInstanz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "wanted_einschreibungen",
                table: "profunda_instanzen",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "wanted_einschreibungen",
                table: "profunda_instanzen");
        }
    }
}
