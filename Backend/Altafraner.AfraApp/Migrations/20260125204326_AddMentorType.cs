using Altafraner.AfraApp.User.Domain.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMentorType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
                .Annotation("Npgsql:Enum:global_permission", "admin,otiumsverantwortlich,profundumsverantwortlich")
                .Annotation("Npgsql:Enum:mentor_type", "gm,im")
                .Annotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .Annotation("Npgsql:Enum:wochentyp", "h,n")
                .OldAnnotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
                .OldAnnotation("Npgsql:Enum:global_permission", "admin,otiumsverantwortlich,profundumsverantwortlich")
                .OldAnnotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .OldAnnotation("Npgsql:Enum:wochentyp", "h,n");

            migrationBuilder.AddColumn<MentorType>(
                name: "type",
                table: "mentor_mentee_relations",
                type: "mentor_type",
                nullable: false,
                defaultValue: MentorType.GM);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "mentor_mentee_relations");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
                .Annotation("Npgsql:Enum:global_permission", "admin,otiumsverantwortlich,profundumsverantwortlich")
                .Annotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .Annotation("Npgsql:Enum:wochentyp", "h,n")
                .OldAnnotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
                .OldAnnotation("Npgsql:Enum:global_permission", "admin,otiumsverantwortlich,profundumsverantwortlich")
                .OldAnnotation("Npgsql:Enum:mentor_type", "gm,im")
                .OldAnnotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .OldAnnotation("Npgsql:Enum:wochentyp", "h,n");
        }
    }
}
