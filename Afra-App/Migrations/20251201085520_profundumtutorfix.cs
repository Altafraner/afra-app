using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class profundumtutorfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TutorId",
                table: "ProfundaInstanzen",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProfundumAnker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Bzeichnung = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundumAnker", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfundumAnkerBewertungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BewertetePersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfundumId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Bewertung = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundumAnkerBewertungen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfundumAnkerBewertungen_Personen_BewertetePersonId",
                        column: x => x.BewertetePersonId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfundumAnkerBewertungen_ProfundaInstanzen_ProfundumId",
                        column: x => x.ProfundumId,
                        principalTable: "ProfundaInstanzen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfundumAnkerBewertungen_ProfundumAnker_AnkerId",
                        column: x => x.AnkerId,
                        principalTable: "ProfundumAnker",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfundaInstanzen_TutorId",
                table: "ProfundaInstanzen",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumAnkerBewertungen_AnkerId",
                table: "ProfundumAnkerBewertungen",
                column: "AnkerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumAnkerBewertungen_BewertetePersonId",
                table: "ProfundumAnkerBewertungen",
                column: "BewertetePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumAnkerBewertungen_ProfundumId",
                table: "ProfundumAnkerBewertungen",
                column: "ProfundumId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaInstanzen_Personen_TutorId",
                table: "ProfundaInstanzen",
                column: "TutorId",
                principalTable: "Personen",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaInstanzen_Personen_TutorId",
                table: "ProfundaInstanzen");

            migrationBuilder.DropTable(
                name: "ProfundumAnkerBewertungen");

            migrationBuilder.DropTable(
                name: "ProfundumAnker");

            migrationBuilder.DropIndex(
                name: "IX_ProfundaInstanzen_TutorId",
                table: "ProfundaInstanzen");

            migrationBuilder.DropColumn(
                name: "TutorId",
                table: "ProfundaInstanzen");
        }
    }
}
