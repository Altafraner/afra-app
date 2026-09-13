using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProfundumBelegWunschEntwurf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "profunda_beleg_wuensche_entwuerfe",
                columns: table => new
                {
                    betroffene_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    profundum_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    einwahl_zeitraum_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rang = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profunda_beleg_wuensche_entwuerfe", x => new { x.profundum_definition_id, x.betroffene_person_id, x.einwahl_zeitraum_id });
                    table.ForeignKey(
                        name: "fk_profunda_beleg_wuensche_entwuerfe_personen_betroffene_perso~",
                        column: x => x.betroffene_person_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_profunda_beleg_wuensche_entwuerfe_profunda_profundum_defini~",
                        column: x => x.profundum_definition_id,
                        principalTable: "profunda",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_profunda_beleg_wuensche_entwuerfe_profundum_einwahl_zeitraeum~",
                        column: x => x.einwahl_zeitraum_id,
                        principalTable: "profundum_einwahl_zeitraeume",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_profunda_beleg_wuensche_entwuerfe_betroffene_person_id",
                table: "profunda_beleg_wuensche_entwuerfe",
                column: "betroffene_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_profunda_beleg_wuensche_entwuerfe_einwahl_zeitraum_id",
                table: "profunda_beleg_wuensche_entwuerfe",
                column: "einwahl_zeitraum_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "profunda_beleg_wuensche_entwuerfe");
        }
    }
}
