using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class AddSindAnwesenheitenKontrolliert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SindAnwesenheitenKontrolliert",
                table: "OtiaTermine",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SindAnwesenheitenFehlernderKontrolliert",
                table: "Blocks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SindAnwesenheitenKontrolliert",
                table: "OtiaTermine");

            migrationBuilder.DropColumn(
                name: "SindAnwesenheitenFehlernderKontrolliert",
                table: "Blocks");
        }
    }
}
