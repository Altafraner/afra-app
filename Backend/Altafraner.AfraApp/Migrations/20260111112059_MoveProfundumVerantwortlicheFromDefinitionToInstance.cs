using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class MoveProfundumVerantwortlicheFromDefinitionToInstance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonProfundumInstanz",
                columns: table => new
                {
                    BetreuteProfundaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerantwortlicheId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonProfundumInstanz", x => new { x.BetreuteProfundaId, x.VerantwortlicheId });
                    table.ForeignKey(
                        name: "FK_PersonProfundumInstanz_Personen_VerantwortlicheId",
                        column: x => x.VerantwortlicheId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonProfundumInstanz_ProfundaInstanzen_BetreuteProfundaId",
                        column: x => x.BetreuteProfundaId,
                        principalTable: "ProfundaInstanzen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfundumInstanz_VerantwortlicheId",
                table: "PersonProfundumInstanz",
                column: "VerantwortlicheId");

            migrationBuilder.Sql("""
                                 INSERT INTO "PersonProfundumInstanz" ("BetreuteProfundaId", "VerantwortlicheId")
                                   SELECT i."Id", ppd."VerantwortlicheId"
                                   FROM "PersonProfundumDefinition" ppd
                                   INNER JOIN "ProfundaInstanzen" i ON i."ProfundumId" = ppd."BetreuteProfundaId"
                                 """);

            migrationBuilder.DropTable(
                name: "PersonProfundumDefinition");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonProfundumDefinition",
                columns: table => new
                {
                    BetreuteProfundaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerantwortlicheId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonProfundumDefinition", x => new { x.BetreuteProfundaId, x.VerantwortlicheId });
                    table.ForeignKey(
                        name: "FK_PersonProfundumDefinition_Personen_VerantwortlicheId",
                        column: x => x.VerantwortlicheId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonProfundumDefinition_Profunda_BetreuteProfundaId",
                        column: x => x.BetreuteProfundaId,
                        principalTable: "Profunda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfundumDefinition_VerantwortlicheId",
                table: "PersonProfundumDefinition",
                column: "VerantwortlicheId");

            // Best effort
            migrationBuilder.Sql("""
                                 INSERT INTO "PersonProfundumDefinition" ("BetreuteProfundaId", "VerantwortlicheId")
                                   SELECT DISTINCT i."ProfundumId", ppi."VerantwortlicheId"
                                   FROM "PersonProfundumInstanz" ppi
                                   INNER JOIN "ProfundaInstanzen" i ON i."Id" = ppi."BetreuteProfundaId"
                                 """);

            migrationBuilder.DropTable(
                name: "PersonProfundumInstanz");
        }
    }
}
