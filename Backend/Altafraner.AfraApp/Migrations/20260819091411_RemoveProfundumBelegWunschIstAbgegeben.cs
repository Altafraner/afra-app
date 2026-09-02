using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProfundumBelegWunschIstAbgegeben : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ist_abgegeben",
                table: "profunda_beleg_wuensche");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ist_abgegeben",
                table: "profunda_beleg_wuensche",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
