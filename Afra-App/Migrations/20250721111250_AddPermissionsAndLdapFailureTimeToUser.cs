using System;
using System.Collections.Generic;
using Afra_App.User.Domain.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionsAndLdapFailureTimeToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
                .Annotation("Npgsql:Enum:global_permission", "admin,otiumsverantwortlich,profundumsverantwortlich")
                .Annotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .Annotation("Npgsql:Enum:wochentyp", "h,n")
                .OldAnnotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
                .OldAnnotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .OldAnnotation("Npgsql:Enum:wochentyp", "h,n");

            migrationBuilder.AddColumn<ISet<GlobalPermission>>(
                name: "GlobalPermissions",
                table: "Personen",
                type: "global_permission[]",
                nullable: false,
                defaultValueSql: "ARRAY[]::global_permission[]");

            migrationBuilder.AddColumn<string>(
                name: "Gruppe",
                table: "Personen",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LdapSyncFailureTime",
                table: "Personen",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GlobalPermissions",
                table: "Personen");

            migrationBuilder.DropColumn(
                name: "Gruppe",
                table: "Personen");

            migrationBuilder.DropColumn(
                name: "LdapSyncFailureTime",
                table: "Personen");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
                .Annotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .Annotation("Npgsql:Enum:wochentyp", "h,n")
                .OldAnnotation("Npgsql:Enum:anwesenheits_status", "anwesend,entschuldigt,fehlend")
                .OldAnnotation("Npgsql:Enum:global_permission", "admin,otiumsverantwortlich,profundumsverantwortlich")
                .OldAnnotation("Npgsql:Enum:person_rolle", "mittelstufe,oberstufe,tutor")
                .OldAnnotation("Npgsql:Enum:wochentyp", "h,n");
        }
    }
}
