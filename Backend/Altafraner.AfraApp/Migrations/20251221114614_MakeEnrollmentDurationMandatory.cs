using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class MakeEnrollmentDurationMandatory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeOnly>(
                name: "Interval_Start",
                table: "OtiaEinschreibungen",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "Interval_Duration",
                table: "OtiaEinschreibungen",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "interval",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeOnly>(
                name: "Interval_Start",
                table: "OtiaEinschreibungen",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "Interval_Duration",
                table: "OtiaEinschreibungen",
                type: "interval",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "interval");
        }
    }
}
