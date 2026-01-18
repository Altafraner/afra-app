using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProfundumBewertung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfundumFeedbackKategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundumFeedbackKategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfundumFeedbackAnker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    KategorieId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundumFeedbackAnker", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfundumFeedbackAnker_ProfundumFeedbackKategories_Kategori~",
                        column: x => x.KategorieId,
                        principalTable: "ProfundumFeedbackKategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfundumFeedbackKategorieProfundumKategorie",
                columns: table => new
                {
                    KategorienId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfundumFeedbackKategorieId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundumFeedbackKategorieProfundumKategorie", x => new { x.KategorienId, x.ProfundumFeedbackKategorieId });
                    table.ForeignKey(
                        name: "FK_ProfundumFeedbackKategorieProfundumKategorie_ProfundaKatego~",
                        column: x => x.KategorienId,
                        principalTable: "ProfundaKategorien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfundumFeedbackKategorieProfundumKategorie_ProfundumFeedb~",
                        column: x => x.ProfundumFeedbackKategorieId,
                        principalTable: "ProfundumFeedbackKategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfundumFeedbackEntries",
                columns: table => new
                {
                    AnkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstanzId = table.Column<Guid>(type: "uuid", nullable: false),
                    BetroffenePersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Grad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundumFeedbackEntries", x => new { x.AnkerId, x.InstanzId, x.BetroffenePersonId });
                    table.ForeignKey(
                        name: "FK_ProfundumFeedbackEntries_Personen_BetroffenePersonId",
                        column: x => x.BetroffenePersonId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfundumFeedbackEntries_ProfundaInstanzen_InstanzId",
                        column: x => x.InstanzId,
                        principalTable: "ProfundaInstanzen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfundumFeedbackEntries_ProfundumFeedbackAnker_AnkerId",
                        column: x => x.AnkerId,
                        principalTable: "ProfundumFeedbackAnker",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumFeedbackAnker_KategorieId",
                table: "ProfundumFeedbackAnker",
                column: "KategorieId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumFeedbackEntries_BetroffenePersonId",
                table: "ProfundumFeedbackEntries",
                column: "BetroffenePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumFeedbackEntries_InstanzId",
                table: "ProfundumFeedbackEntries",
                column: "InstanzId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumFeedbackKategorieProfundumKategorie_ProfundumFeedb~",
                table: "ProfundumFeedbackKategorieProfundumKategorie",
                column: "ProfundumFeedbackKategorieId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfundumFeedbackEntries");

            migrationBuilder.DropTable(
                name: "ProfundumFeedbackKategorieProfundumKategorie");

            migrationBuilder.DropTable(
                name: "ProfundumFeedbackAnker");

            migrationBuilder.DropTable(
                name: "ProfundumFeedbackKategories");
        }
    }
}
