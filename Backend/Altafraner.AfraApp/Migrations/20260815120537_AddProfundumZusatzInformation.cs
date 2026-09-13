using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProfundumZusatzInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "profundum_zusatz_informationen",
                columns: table => new
                {
                    betroffene_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    einwahl_zeitraum_id = table.Column<Guid>(type: "uuid", nullable: false),
                    information = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profundum_zusatz_informationen", x => new { x.betroffene_person_id, x.einwahl_zeitraum_id });
                    table.ForeignKey(
                        name: "fk_profundum_zusatz_informationen_personen_betroffene_person_id",
                        column: x => x.betroffene_person_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_profundum_zusatz_informationen_profundum_einwahl_zeitraeume~",
                        column: x => x.einwahl_zeitraum_id,
                        principalTable: "profundum_einwahl_zeitraeume",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_profundum_zusatz_informationen_einwahl_zeitraum_id",
                table: "profundum_zusatz_informationen",
                column: "einwahl_zeitraum_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "profundum_zusatz_informationen");
        }
    }
}
