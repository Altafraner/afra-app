using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProfundumEinwahlZeitraum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EinwahlMöglich",
                table: "ProfundaSlots");

            migrationBuilder.AddColumn<Guid>(
                name: "EinwahlZeitraumId",
                table: "ProfundaSlots",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ProfundumEinwahlZeitraeume",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EinwahlStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EinwahlStop = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundumEinwahlZeitraeume", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfundaSlots_EinwahlZeitraumId",
                table: "ProfundaSlots",
                column: "EinwahlZeitraumId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaSlots_ProfundumEinwahlZeitraeume_EinwahlZeitraumId",
                table: "ProfundaSlots",
                column: "EinwahlZeitraumId",
                principalTable: "ProfundumEinwahlZeitraeume",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaSlots_ProfundumEinwahlZeitraeume_EinwahlZeitraumId",
                table: "ProfundaSlots");

            migrationBuilder.DropTable(
                name: "ProfundumEinwahlZeitraeume");

            migrationBuilder.DropIndex(
                name: "IX_ProfundaSlots_EinwahlZeitraumId",
                table: "ProfundaSlots");

            migrationBuilder.DropColumn(
                name: "EinwahlZeitraumId",
                table: "ProfundaSlots");

            migrationBuilder.AddColumn<bool>(
                name: "EinwahlMöglich",
                table: "ProfundaSlots",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
