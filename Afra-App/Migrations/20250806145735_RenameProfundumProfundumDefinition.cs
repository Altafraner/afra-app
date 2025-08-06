using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class RenameProfundumProfundumDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtiumPerson");

            migrationBuilder.CreateTable(
                name: "OtiumDefinitionPerson",
                columns: table => new
                {
                    VerantwortlicheId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerwalteteOtiaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiumDefinitionPerson", x => new { x.VerantwortlicheId, x.VerwalteteOtiaId });
                    table.ForeignKey(
                        name: "FK_OtiumDefinitionPerson_Otia_VerwalteteOtiaId",
                        column: x => x.VerwalteteOtiaId,
                        principalTable: "Otia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtiumDefinitionPerson_Personen_VerantwortlicheId",
                        column: x => x.VerantwortlicheId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OtiumDefinitionPerson_VerwalteteOtiaId",
                table: "OtiumDefinitionPerson",
                column: "VerwalteteOtiaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtiumDefinitionPerson");

            migrationBuilder.CreateTable(
                name: "OtiumPerson",
                columns: table => new
                {
                    VerantwortlicheId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerwalteteOtiaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiumPerson", x => new { x.VerantwortlicheId, x.VerwalteteOtiaId });
                    table.ForeignKey(
                        name: "FK_OtiumPerson_Otia_VerwalteteOtiaId",
                        column: x => x.VerwalteteOtiaId,
                        principalTable: "Otia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtiumPerson_Personen_VerantwortlicheId",
                        column: x => x.VerantwortlicheId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OtiumPerson_VerwalteteOtiaId",
                table: "OtiumPerson",
                column: "VerwalteteOtiaId");
        }
    }
}
