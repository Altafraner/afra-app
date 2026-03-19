using System;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceRework : Migration
    {
        /// <inheritdoc />
       protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TYPE anwesenheits_status RENAME TO attendance_state;
                """);

            // Create the new scope enum.
            migrationBuilder.Sql("""
                CREATE TYPE attendance_scope AS ENUM ('general', 'otium', 'profundum');
                """);

            migrationBuilder.CreateTable(
                name: "attendance_event_status",
                columns: table => new
                {
                    scope = table.Column<AttendanceScope>(type: "attendance_scope", nullable: false),
                    slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attendance_event_status", x => new { x.scope, x.slot_id, x.event_id });
                });

            migrationBuilder.CreateTable(
                name: "attendance_notes",
                columns: table => new
                {
                    scope = table.Column<AttendanceScope>(type: "attendance_scope", nullable: false),
                    slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attendance_notes", x => new { x.scope, x.slot_id, x.student_id, x.author_id });
                    table.ForeignKey(
                        name: "fk_attendance_notes_personen_author_id",
                        column: x => x.author_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_attendance_notes_personen_student_id",
                        column: x => x.student_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attendances",
                columns: table => new
                {
                    scope = table.Column<AttendanceScope>(type: "attendance_scope", maxLength: 10, nullable: false),
                    slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<AttendanceState>(type: "attendance_state", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attendances", x => new { x.scope, x.slot_id, x.student_id });
                    table.ForeignKey(
                        name: "fk_attendances_personen_student_id",
                        column: x => x.student_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_attendance_notes_author_id",
                table: "attendance_notes",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_attendance_notes_student_id",
                table: "attendance_notes",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_attendances_student_id",
                table: "attendances",
                column: "student_id");

            migrationBuilder.Sql("""
                INSERT INTO attendances (scope, slot_id, student_id, status)
                SELECT
                    'otium'::attendance_scope,
                    block_id,
                    student_id,
                    status
                FROM otia_anwesenheiten;
                """);

            migrationBuilder.Sql("""
                INSERT INTO attendance_notes (scope, slot_id, student_id, author_id, content, created_at, last_modified)
                SELECT
                    'otium'::attendance_scope,
                    block_id,
                    student_id,
                    author_id,
                    content,
                    created_at,
                    last_modified
                FROM otia_einschreibungs_notizen;
                """);

            migrationBuilder.Sql("""
                INSERT INTO attendance_event_status (scope, slot_id, event_id, status)
                SELECT
                    'otium'::attendance_scope,
                    block_id,
                    id,
                    sind_anwesenheiten_kontrolliert
                FROM otia_termine;
                """);

            migrationBuilder.Sql("""
                INSERT INTO attendance_event_status (scope, slot_id, event_id, status)
                SELECT
                    'otium'::attendance_scope,
                    id,
                    '00000000-0000-0000-0000-000000000000'::uuid,
                    sind_anwesenheiten_fehlernder_kontrolliert
                FROM blocks;
                """);

            migrationBuilder.DropTable(
                name: "otia_anwesenheiten");

            migrationBuilder.DropTable(
                name: "otia_einschreibungs_notizen");

            migrationBuilder.DropColumn(
                name: "sind_anwesenheiten_kontrolliert",
                table: "otia_termine");

            migrationBuilder.DropColumn(
                name: "sind_anwesenheiten_fehlernder_kontrolliert",
                table: "blocks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This is destructive! I recommend backing the database up before trying this.
            migrationBuilder.DropTable(
                name: "attendance_event_status");

            migrationBuilder.DropTable(
                name: "attendance_notes");

            migrationBuilder.DropTable(
                name: "attendances");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
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

            migrationBuilder.AddColumn<bool>(
                name: "sind_anwesenheiten_kontrolliert",
                table: "otia_termine",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "sind_anwesenheiten_fehlernder_kontrolliert",
                table: "blocks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "otia_anwesenheiten",
                columns: table => new
                {
                    block_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "anwesenheits_status", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_otia_anwesenheiten", x => new { x.block_id, x.student_id });
                    table.ForeignKey(
                        name: "fk_otia_anwesenheiten_blocks_block_id",
                        column: x => x.block_id,
                        principalTable: "blocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_otia_anwesenheiten_personen_student_id",
                        column: x => x.student_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "otia_einschreibungs_notizen",
                columns: table => new
                {
                    block_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_otia_einschreibungs_notizen", x => new { x.block_id, x.student_id, x.author_id });
                    table.ForeignKey(
                        name: "fk_otia_einschreibungs_notizen_blocks_block_id",
                        column: x => x.block_id,
                        principalTable: "blocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_otia_einschreibungs_notizen_personen_author_id",
                        column: x => x.author_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_otia_einschreibungs_notizen_personen_student_id",
                        column: x => x.student_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_otia_anwesenheiten_student_id",
                table: "otia_anwesenheiten",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_otia_einschreibungs_notizen_author_id",
                table: "otia_einschreibungs_notizen",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_otia_einschreibungs_notizen_student_id",
                table: "otia_einschreibungs_notizen",
                column: "student_id");
        }
    }
}
