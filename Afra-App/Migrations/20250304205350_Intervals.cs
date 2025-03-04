using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class Intervals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ende",
                table: "OtiaEinschreibungen");

            migrationBuilder.RenameColumn(
                name: "Start",
                table: "OtiaEinschreibungen",
                newName: "Interval_Start");

            migrationBuilder.AddColumn<bool>(
                name: "Required",
                table: "OtiaKategorien",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Interval_Duration",
                table: "OtiaEinschreibungen",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Required",
                table: "OtiaKategorien");

            migrationBuilder.DropColumn(
                name: "Interval_Duration",
                table: "OtiaEinschreibungen");

            migrationBuilder.RenameColumn(
                name: "Interval_Start",
                table: "OtiaEinschreibungen",
                newName: "Start");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Ende",
                table: "OtiaEinschreibungen",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }
    }
}
