using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class Schultage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Datum",
                table: "OtiaTermine",
                newName: "SchultagDatum");

            migrationBuilder.AddColumn<int>(
                name: "Wochentyp",
                table: "OtiaWiederholungen",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Schultage",
                columns: table => new
                {
                    Datum = table.Column<DateOnly>(type: "date", nullable: false),
                    Wochentyp = table.Column<int>(type: "integer", nullable: false),
                    OtiumsBlock = table.Column<bool[]>(type: "boolean[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schultage", x => x.Datum);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OtiaTermine_SchultagDatum",
                table: "OtiaTermine",
                column: "SchultagDatum");

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaTermine_Schultage_SchultagDatum",
                table: "OtiaTermine",
                column: "SchultagDatum",
                principalTable: "Schultage",
                principalColumn: "Datum",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OtiaTermine_Schultage_SchultagDatum",
                table: "OtiaTermine");

            migrationBuilder.DropTable(
                name: "Schultage");

            migrationBuilder.DropIndex(
                name: "IX_OtiaTermine_SchultagDatum",
                table: "OtiaTermine");

            migrationBuilder.DropColumn(
                name: "Wochentyp",
                table: "OtiaWiederholungen");

            migrationBuilder.RenameColumn(
                name: "SchultagDatum",
                table: "OtiaTermine",
                newName: "Datum");
        }
    }
}
