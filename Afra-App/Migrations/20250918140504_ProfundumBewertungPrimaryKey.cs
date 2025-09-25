using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class ProfundumBewertungPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfundumsBewertungKriterien",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Bezeichnung = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundumsBewertungKriterien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfundumBewertungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KriteriumId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstanzId = table.Column<Guid>(type: "uuid", nullable: false),
                    BetroffenePersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Grad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundumBewertungen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfundumBewertungen_Personen_BetroffenePersonId",
                        column: x => x.BetroffenePersonId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfundumBewertungen_ProfundaInstanzen_InstanzId",
                        column: x => x.InstanzId,
                        principalTable: "ProfundaInstanzen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfundumBewertungen_ProfundumsBewertungKriterien_KriteriumId",
                        column: x => x.KriteriumId,
                        principalTable: "ProfundumsBewertungKriterien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumBewertungen_BetroffenePersonId",
                table: "ProfundumBewertungen",
                column: "BetroffenePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumBewertungen_InstanzId",
                table: "ProfundumBewertungen",
                column: "InstanzId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumBewertungen_KriteriumId",
                table: "ProfundumBewertungen",
                column: "KriteriumId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfundumBewertungen");

            migrationBuilder.DropTable(
                name: "ProfundumsBewertungKriterien");
        }
    }
}
