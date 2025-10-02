using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class ProfundumKategorieMaxProEinwahl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profunda_ProfundumKategorie_KategorieId",
                table: "Profunda");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundumKategorie",
                table: "ProfundumKategorie");

            migrationBuilder.RenameTable(
                name: "ProfundumKategorie",
                newName: "ProfundaKategorien");

            migrationBuilder.AddColumn<int>(
                name: "MaxProEinwahl",
                table: "ProfundaKategorien",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundaKategorien",
                table: "ProfundaKategorien",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Profunda_ProfundaKategorien_KategorieId",
                table: "Profunda",
                column: "KategorieId",
                principalTable: "ProfundaKategorien",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profunda_ProfundaKategorien_KategorieId",
                table: "Profunda");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundaKategorien",
                table: "ProfundaKategorien");

            migrationBuilder.DropColumn(
                name: "MaxProEinwahl",
                table: "ProfundaKategorien");

            migrationBuilder.RenameTable(
                name: "ProfundaKategorien",
                newName: "ProfundumKategorie");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundumKategorie",
                table: "ProfundumKategorie",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Profunda_ProfundumKategorie_KategorieId",
                table: "Profunda",
                column: "KategorieId",
                principalTable: "ProfundumKategorie",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
