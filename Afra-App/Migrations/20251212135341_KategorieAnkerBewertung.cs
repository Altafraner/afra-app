using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class KategorieAnkerBewertung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KategorieId",
                table: "ProfundumAnker",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumAnker_KategorieId",
                table: "ProfundumAnker",
                column: "KategorieId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundumAnker_ProfundaKategorien_KategorieId",
                table: "ProfundumAnker",
                column: "KategorieId",
                principalTable: "ProfundaKategorien",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumAnker_ProfundaKategorien_KategorieId",
                table: "ProfundumAnker");

            migrationBuilder.DropIndex(
                name: "IX_ProfundumAnker_KategorieId",
                table: "ProfundumAnker");

            migrationBuilder.DropColumn(
                name: "KategorieId",
                table: "ProfundumAnker");
        }
    }
}
