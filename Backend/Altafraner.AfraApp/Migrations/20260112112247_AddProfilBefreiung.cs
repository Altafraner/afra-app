using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilBefreiung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfundumProfilBefreiungen",
                columns: table => new
                {
                    BetroffenePersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Jahr = table.Column<int>(type: "integer", nullable: false),
                    Quartal = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundumProfilBefreiungen", x => new { x.BetroffenePersonId, x.Jahr, x.Quartal });
                    table.ForeignKey(
                        name: "FK_ProfundumProfilBefreiungen_Personen_BetroffenePersonId",
                        column: x => x.BetroffenePersonId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfundumProfilBefreiungen");
        }
    }
}
