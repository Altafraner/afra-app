using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class Ldap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LdapObjectId",
                table: "Personen",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LdapSyncTime",
                table: "Personen",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LdapObjectId",
                table: "Personen");

            migrationBuilder.DropColumn(
                name: "LdapSyncTime",
                table: "Personen");
        }
    }
}
