using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class AnkerFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KriteriumId",
                table: "ProfundumAnkerBewertungen",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumAnkerBewertungen_KriteriumId",
                table: "ProfundumAnkerBewertungen",
                column: "KriteriumId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundumAnkerBewertungen_ProfundumsBewertungKriterien_Krit~",
                table: "ProfundumAnkerBewertungen",
                column: "KriteriumId",
                principalTable: "ProfundumsBewertungKriterien",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumAnkerBewertungen_ProfundumsBewertungKriterien_Krit~",
                table: "ProfundumAnkerBewertungen");

            migrationBuilder.DropIndex(
                name: "IX_ProfundumAnkerBewertungen_KriteriumId",
                table: "ProfundumAnkerBewertungen");

            migrationBuilder.DropColumn(
                name: "KriteriumId",
                table: "ProfundumAnkerBewertungen");
        }
    }
}
