using System;
using Afra_App.Otium.Domain.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
                .Annotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .Annotation("Npgsql:Enum:wochentyp", "h,n")
                .OldAnnotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .OldAnnotation("Npgsql:Enum:wochentyp", "h,n");

            migrationBuilder.CreateTable(
                name: "OtiaAnwesenheiten",
                columns: table => new
                {
                    BlockId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<AnwesenheitsStatus>(type: "anwesenheits_status", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiaAnwesenheiten", x => new { x.BlockId, x.StudentId });
                    table.ForeignKey(
                        name: "FK_OtiaAnwesenheiten_Blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtiaAnwesenheiten_Personen_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OtiaAnwesenheiten_StudentId",
                table: "OtiaAnwesenheiten",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtiaAnwesenheiten");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .Annotation("Npgsql:Enum:wochentyp", "h,n")
                .OldAnnotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
                .OldAnnotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .OldAnnotation("Npgsql:Enum:wochentyp", "h,n");
        }
    }
}
