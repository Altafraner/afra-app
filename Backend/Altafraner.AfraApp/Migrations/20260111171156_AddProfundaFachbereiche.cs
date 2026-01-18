using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProfundaFachbereiche : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfundumFeedbackKategorieProfundumKategorie");

            migrationBuilder.CreateTable(
                name: "ProfundaFachbereiche",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundaFachbereiche", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfundumDefinitionProfundumFachbereich",
                columns: table => new
                {
                    FachbereicheId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfundaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundumDefinitionProfundumFachbereich", x => new { x.FachbereicheId, x.ProfundaId });
                    table.ForeignKey(
                        name: "FK_ProfundumDefinitionProfundumFachbereich_ProfundaFachbereich~",
                        column: x => x.FachbereicheId,
                        principalTable: "ProfundaFachbereiche",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfundumDefinitionProfundumFachbereich_Profunda_ProfundaId",
                        column: x => x.ProfundaId,
                        principalTable: "Profunda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfundumFachbereichProfundumFeedbackKategorie",
                columns: table => new
                {
                    FachbereicheId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfundumFeedbackKategorieId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundumFachbereichProfundumFeedbackKategorie", x => new { x.FachbereicheId, x.ProfundumFeedbackKategorieId });
                    table.ForeignKey(
                        name: "FK_ProfundumFachbereichProfundumFeedbackKategorie_ProfundaFach~",
                        column: x => x.FachbereicheId,
                        principalTable: "ProfundaFachbereiche",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfundumFachbereichProfundumFeedbackKategorie_ProfundumFee~",
                        column: x => x.ProfundumFeedbackKategorieId,
                        principalTable: "ProfundumFeedbackKategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumDefinitionProfundumFachbereich_ProfundaId",
                table: "ProfundumDefinitionProfundumFachbereich",
                column: "ProfundaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumFachbereichProfundumFeedbackKategorie_ProfundumFee~",
                table: "ProfundumFachbereichProfundumFeedbackKategorie",
                column: "ProfundumFeedbackKategorieId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfundumDefinitionProfundumFachbereich");

            migrationBuilder.DropTable(
                name: "ProfundumFachbereichProfundumFeedbackKategorie");

            migrationBuilder.DropTable(
                name: "ProfundaFachbereiche");

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

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumFeedbackKategorieProfundumKategorie_ProfundumFeedb~",
                table: "ProfundumFeedbackKategorieProfundumKategorie",
                column: "ProfundumFeedbackKategorieId");
        }
    }
}
