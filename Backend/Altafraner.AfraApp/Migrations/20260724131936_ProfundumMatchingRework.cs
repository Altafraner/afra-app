using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class ProfundumMatchingRework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_profunda_beleg_wuensche_profunda_instanzen_profundum_instanz~",
                table: "profunda_beleg_wuensche");

            migrationBuilder.DropTable(
                name: "profundum_profil_befreiungen");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda_beleg_wuensche",
                table: "profunda_beleg_wuensche");

            migrationBuilder.RenameColumn(
                name: "stufe",
                table: "profunda_beleg_wuensche",
                newName: "rang");

            migrationBuilder.RenameColumn(
                name: "profundum_instanz_id",
                table: "profunda_beleg_wuensche",
                newName: "profundum_definition_id");

            migrationBuilder.AddColumn<bool>(
                name: "ist_abgegeben",
                table: "profunda_beleg_wuensche",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "erlaubt_partnerwahl",
                table: "profunda",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda_beleg_wuensche",
                table: "profunda_beleg_wuensche",
                columns: new[] { "profundum_definition_id", "betroffene_person_id", "einwahl_zeitraum_id" });

            migrationBuilder.CreateTable(
                name: "person_gruppen_historien",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gruppe = table.Column<string>(type: "text", nullable: true),
                    gueltig_ab = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_person_gruppen_historien", x => x.id);
                    table.ForeignKey(
                        name: "fk_person_gruppen_historien_personen_person_id",
                        column: x => x.person_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "profundum_partner_einladungen",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profundum_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    einwahl_zeitraum_id = table.Column<Guid>(type: "uuid", nullable: false),
                    initiator_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profundum_partner_einladungen", x => x.id);
                    table.ForeignKey(
                        name: "fk_profundum_partner_einladungen_personen_initiator_person_id",
                        column: x => x.initiator_person_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_profundum_partner_einladungen_profunda_profundum_definition~",
                        column: x => x.profundum_definition_id,
                        principalTable: "profunda",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_profundum_partner_einladungen_profundum_einwahl_zeitraeume_~",
                        column: x => x.einwahl_zeitraum_id,
                        principalTable: "profundum_einwahl_zeitraeume",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "profundum_partner_wuensche",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profundum_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    einwahl_zeitraum_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_a_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_b_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profundum_partner_wuensche", x => x.id);
                    table.ForeignKey(
                        name: "fk_profundum_partner_wuensche_personen_person_a_id",
                        column: x => x.person_a_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_profundum_partner_wuensche_personen_person_b_id",
                        column: x => x.person_b_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_profundum_partner_wuensche_profunda_profundum_definition_id",
                        column: x => x.profundum_definition_id,
                        principalTable: "profunda",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_profundum_partner_wuensche_profundum_einwahl_zeitraeume_ein~",
                        column: x => x.einwahl_zeitraum_id,
                        principalTable: "profundum_einwahl_zeitraeume",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_person_gruppen_historien_person_id_gueltig_ab",
                table: "person_gruppen_historien",
                columns: new[] { "person_id", "gueltig_ab" });

            migrationBuilder.CreateIndex(
                name: "ix_profundum_partner_einladungen_einwahl_zeitraum_id",
                table: "profundum_partner_einladungen",
                column: "einwahl_zeitraum_id");

            migrationBuilder.CreateIndex(
                name: "ix_profundum_partner_einladungen_initiator_person_id",
                table: "profundum_partner_einladungen",
                column: "initiator_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_profundum_partner_einladungen_profundum_definition_id",
                table: "profundum_partner_einladungen",
                column: "profundum_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_profundum_partner_wuensche_einwahl_zeitraum_id",
                table: "profundum_partner_wuensche",
                column: "einwahl_zeitraum_id");

            migrationBuilder.CreateIndex(
                name: "ix_profundum_partner_wuensche_person_a_id",
                table: "profundum_partner_wuensche",
                column: "person_a_id");

            migrationBuilder.CreateIndex(
                name: "ix_profundum_partner_wuensche_person_b_id",
                table: "profundum_partner_wuensche",
                column: "person_b_id");

            migrationBuilder.CreateIndex(
                name: "ix_profundum_partner_wuensche_profundum_definition_id",
                table: "profundum_partner_wuensche",
                column: "profundum_definition_id");

            migrationBuilder.AddForeignKey(
                name: "fk_profunda_beleg_wuensche_profunda_profundum_definition_id",
                table: "profunda_beleg_wuensche",
                column: "profundum_definition_id",
                principalTable: "profunda",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_profunda_beleg_wuensche_profunda_profundum_definition_id",
                table: "profunda_beleg_wuensche");

            migrationBuilder.DropTable(
                name: "person_gruppen_historien");

            migrationBuilder.DropTable(
                name: "profundum_partner_einladungen");

            migrationBuilder.DropTable(
                name: "profundum_partner_wuensche");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda_beleg_wuensche",
                table: "profunda_beleg_wuensche");

            migrationBuilder.DropColumn(
                name: "ist_abgegeben",
                table: "profunda_beleg_wuensche");

            migrationBuilder.DropColumn(
                name: "erlaubt_partnerwahl",
                table: "profunda");

            migrationBuilder.RenameColumn(
                name: "rang",
                table: "profunda_beleg_wuensche",
                newName: "stufe");

            migrationBuilder.RenameColumn(
                name: "profundum_definition_id",
                table: "profunda_beleg_wuensche",
                newName: "profundum_instanz_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda_beleg_wuensche",
                table: "profunda_beleg_wuensche",
                columns: new[] { "profundum_instanz_id", "betroffene_person_id", "stufe" });

            migrationBuilder.CreateTable(
                name: "profundum_profil_befreiungen",
                columns: table => new
                {
                    betroffene_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    jahr = table.Column<int>(type: "integer", nullable: false),
                    quartal = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profundum_profil_befreiungen", x => new { x.betroffene_person_id, x.jahr, x.quartal });
                    table.ForeignKey(
                        name: "fk_profundum_profil_befreiungen_personen_betroffene_person_id",
                        column: x => x.betroffene_person_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "fk_profunda_beleg_wuensche_profunda_instanzen_profundum_instanz~",
                table: "profunda_beleg_wuensche",
                column: "profundum_instanz_id",
                principalTable: "profunda_instanzen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
