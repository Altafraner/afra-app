using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class ProfundumBelegWunschEinwahlZeitraum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EinwahlZeitraumId",
                table: "ProfundaBelegWuensche",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("01989cef-c4b2-7dde-a5af-6314431a2822"));

            migrationBuilder.CreateIndex(
                name: "IX_ProfundaBelegWuensche_EinwahlZeitraumId",
                table: "ProfundaBelegWuensche",
                column: "EinwahlZeitraumId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaBelegWuensche_ProfundumEinwahlZeitraeume_EinwahlZei~",
                table: "ProfundaBelegWuensche",
                column: "EinwahlZeitraumId",
                principalTable: "ProfundumEinwahlZeitraeume",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaBelegWuensche_ProfundumEinwahlZeitraeume_EinwahlZei~",
                table: "ProfundaBelegWuensche");

            migrationBuilder.DropIndex(
                name: "IX_ProfundaBelegWuensche_EinwahlZeitraumId",
                table: "ProfundaBelegWuensche");

            migrationBuilder.DropColumn(
                name: "EinwahlZeitraumId",
                table: "ProfundaBelegWuensche");
        }
    }
}
