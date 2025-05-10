using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEinschreibungInterval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Blocks_SchultagKey_Nummer",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "Interval_Duration",
                table: "OtiaEinschreibungen");

            migrationBuilder.DropColumn(
                name: "Interval_Start",
                table: "OtiaEinschreibungen");

            migrationBuilder.DropColumn(
                name: "Nummer",
                table: "Blocks");

            migrationBuilder.AddColumn<char>(
                name: "SchemaId",
                table: "Blocks",
                type: "character(1)",
                nullable: false,
                defaultValueSql: "''");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_SchultagKey_SchemaId",
                table: "Blocks",
                columns: new[] { "SchultagKey", "SchemaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Blocks_SchultagKey_SchemaId",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "SchemaId",
                table: "Blocks");

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

            migrationBuilder.AddColumn<short>(
                name: "Nummer",
                table: "Blocks",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_SchultagKey_Nummer",
                table: "Blocks",
                columns: new[] { "SchultagKey", "Nummer" },
                unique: true);
        }
    }
}
