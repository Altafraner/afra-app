using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddModifedCreatedTimesToOtiumTerminOtiumEinschreibung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "OtiaTermine",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "OtiaTermine",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "OtiaEinschreibungen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "OtiaEinschreibungen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "OtiaTermine");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "OtiaTermine");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "OtiaEinschreibungen");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "OtiaEinschreibungen");
        }
    }
}
