using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class ProfundumAddCreateModified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ProfundaInstanzen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 12, 20, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "ProfundaInstanzen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 12, 20, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ProfundaEinschreibungen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 12, 20, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "ProfundaEinschreibungen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 12, 20, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Profunda",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 12, 20, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Profunda",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 12, 20, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ProfundaInstanzen");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "ProfundaInstanzen");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Profunda");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Profunda");
        }
    }
}
