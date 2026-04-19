using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCevexSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attendance_entry_type", "automatic,manual")
                .Annotation("Npgsql:Enum:attendance_scope", "general,otium,profundum")
                .Annotation("Npgsql:Enum:attendance_state", "anwesend,entschuldigt,fehlend")
                .Annotation("Npgsql:Enum:global_permission", "admin,otiumsverantwortlich,profundumsverantwortlich")
                .Annotation("Npgsql:Enum:mentor_type", "gm,im")
                .Annotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .Annotation("Npgsql:Enum:wochentyp", "h,n")
                .OldAnnotation("Npgsql:Enum:attendance_scope", "general,otium,profundum")
                .OldAnnotation("Npgsql:Enum:attendance_state", "anwesend,entschuldigt,fehlend")
                .OldAnnotation("Npgsql:Enum:global_permission", "admin,otiumsverantwortlich,profundumsverantwortlich")
                .OldAnnotation("Npgsql:Enum:mentor_type", "gm,im")
                .OldAnnotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .OldAnnotation("Npgsql:Enum:wochentyp", "h,n");

            migrationBuilder.AddColumn<string>(
                name: "cevex_id",
                table: "personen",
                type: "character varying(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "cevex_id_manually_entered",
                table: "personen",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "cevex_sync_failure_time",
                table: "personen",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cevex_id",
                table: "personen");

            migrationBuilder.DropColumn(
                name: "cevex_id_manually_entered",
                table: "personen");

            migrationBuilder.DropColumn(
                name: "cevex_sync_failure_time",
                table: "personen");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attendance_scope", "general,otium,profundum")
                .Annotation("Npgsql:Enum:attendance_state", "anwesend,entschuldigt,fehlend")
                .Annotation("Npgsql:Enum:global_permission", "admin,otiumsverantwortlich,profundumsverantwortlich")
                .Annotation("Npgsql:Enum:mentor_type", "gm,im")
                .Annotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .Annotation("Npgsql:Enum:wochentyp", "h,n")
                .OldAnnotation("Npgsql:Enum:attendance_entry_type", "automatic,manual")
                .OldAnnotation("Npgsql:Enum:attendance_scope", "general,otium,profundum")
                .OldAnnotation("Npgsql:Enum:attendance_state", "anwesend,entschuldigt,fehlend")
                .OldAnnotation("Npgsql:Enum:global_permission", "admin,otiumsverantwortlich,profundumsverantwortlich")
                .OldAnnotation("Npgsql:Enum:mentor_type", "gm,im")
                .OldAnnotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .OldAnnotation("Npgsql:Enum:wochentyp", "h,n");
        }
    }
}
