using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class ReintroduceEinschreibungIntervall : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "Interval_Duration",
                table: "OtiaEinschreibungen",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Interval_Start",
                table: "OtiaEinschreibungen",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Interval_Duration",
                table: "OtiaEinschreibungen");

            migrationBuilder.DropColumn(
                name: "Interval_Start",
                table: "OtiaEinschreibungen");
        }
    }
}
