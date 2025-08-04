using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class AddProfundumKategorien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilProfundum",
                table: "Profunda");

            migrationBuilder.AddColumn<Guid>(
                name: "KategorieId",
                table: "Profunda",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ProfundumKategorie",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Bezeichnung = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProfilProfundum = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundumKategorie", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profunda_KategorieId",
                table: "Profunda",
                column: "KategorieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profunda_ProfundumKategorie_KategorieId",
                table: "Profunda",
                column: "KategorieId",
                principalTable: "ProfundumKategorie",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profunda_ProfundumKategorie_KategorieId",
                table: "Profunda");

            migrationBuilder.DropTable(
                name: "ProfundumKategorie");

            migrationBuilder.DropIndex(
                name: "IX_Profunda_KategorieId",
                table: "Profunda");

            migrationBuilder.DropColumn(
                name: "KategorieId",
                table: "Profunda");

            migrationBuilder.AddColumn<bool>(
                name: "ProfilProfundum",
                table: "Profunda",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
