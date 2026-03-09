using System;
using Altafraner.AfraApp.Freistellung.Domain.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveRequestModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
                .Annotation("Npgsql:Enum:entscheidungs_status", "abgelehnt,ausstehend,genehmigt")
                .Annotation("Npgsql:Enum:freistellungs_status", "abgelehnt,alle_lehrer_genehmigt,bestaetigt,gestellt,schulleiter_abgelehnt,schulleiter_bestaetigt,sekretariat_abgelehnt")
                .Annotation("Npgsql:Enum:global_permission", "admin,otiumsverantwortlich,profundumsverantwortlich,schulleiter,sekretariat")
                .Annotation("Npgsql:Enum:mentor_type", "gm,im")
                .Annotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .Annotation("Npgsql:Enum:wochentyp", "h,n")
                .OldAnnotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
                .OldAnnotation("Npgsql:Enum:global_permission", "admin,otiumsverantwortlich,profundumsverantwortlich")
                .OldAnnotation("Npgsql:Enum:mentor_type", "gm,im")
                .OldAnnotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .OldAnnotation("Npgsql:Enum:wochentyp", "h,n");

            migrationBuilder.CreateTable(
                name: "freistellungsantraege",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grund = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    beschreibung = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    von = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    bis = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<FreistellungsStatus>(type: "freistellungs_status", nullable: false),
                    erstellt_am = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sekretariat_kommentar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    schulleiter_kommentar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_freistellungsantraege", x => x.id);
                    table.ForeignKey(
                        name: "fk_freistellungsantraege_personen_student_id",
                        column: x => x.student_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "betroffene_stunden",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    freistellungsantrag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    datum = table.Column<DateOnly>(type: "date", nullable: false),
                    block = table.Column<int>(type: "integer", nullable: false),
                    fach = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    lehrer_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_betroffene_stunden", x => x.id);
                    table.ForeignKey(
                        name: "fk_betroffene_stunden_freistellungsantraege_freistellungsantra~",
                        column: x => x.freistellungsantrag_id,
                        principalTable: "freistellungsantraege",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_betroffene_stunden_personen_lehrer_id",
                        column: x => x.lehrer_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lehrer_entscheidungen",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    freistellungsantrag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lehrer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<EntscheidungsStatus>(type: "entscheidungs_status", nullable: false),
                    kommentar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    entschieden_am = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lehrer_entscheidungen", x => x.id);
                    table.ForeignKey(
                        name: "fk_lehrer_entscheidungen_freistellungsantraege_freistellungsan~",
                        column: x => x.freistellungsantrag_id,
                        principalTable: "freistellungsantraege",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lehrer_entscheidungen_personen_lehrer_id",
                        column: x => x.lehrer_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_betroffene_stunden_freistellungsantrag_id",
                table: "betroffene_stunden",
                column: "freistellungsantrag_id");

            migrationBuilder.CreateIndex(
                name: "ix_betroffene_stunden_lehrer_id",
                table: "betroffene_stunden",
                column: "lehrer_id");

            migrationBuilder.CreateIndex(
                name: "ix_freistellungsantraege_student_id",
                table: "freistellungsantraege",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_lehrer_entscheidungen_freistellungsantrag_id",
                table: "lehrer_entscheidungen",
                column: "freistellungsantrag_id");

            migrationBuilder.CreateIndex(
                name: "ix_lehrer_entscheidungen_lehrer_id",
                table: "lehrer_entscheidungen",
                column: "lehrer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "betroffene_stunden");

            migrationBuilder.DropTable(
                name: "lehrer_entscheidungen");

            migrationBuilder.DropTable(
                name: "freistellungsantraege");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
                .Annotation("Npgsql:Enum:global_permission", "admin,otiumsverantwortlich,profundumsverantwortlich")
                .Annotation("Npgsql:Enum:mentor_type", "gm,im")
                .Annotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .Annotation("Npgsql:Enum:wochentyp", "h,n")
                .OldAnnotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
                .OldAnnotation("Npgsql:Enum:entscheidungs_status", "abgelehnt,ausstehend,genehmigt")
                .OldAnnotation("Npgsql:Enum:freistellungs_status", "abgelehnt,alle_lehrer_genehmigt,bestaetigt,gestellt,schulleiter_abgelehnt,schulleiter_bestaetigt,sekretariat_abgelehnt")
                .OldAnnotation("Npgsql:Enum:global_permission", "admin,otiumsverantwortlich,profundumsverantwortlich,schulleiter,sekretariat")
                .OldAnnotation("Npgsql:Enum:mentor_type", "gm,im")
                .OldAnnotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .OldAnnotation("Npgsql:Enum:wochentyp", "h,n");
        }
    }
}
