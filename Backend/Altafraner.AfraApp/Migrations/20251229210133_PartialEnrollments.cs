using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class PartialEnrollments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundaEinschreibungen",
                table: "ProfundaEinschreibungen");

            migrationBuilder.AddColumn<Guid>(
                name: "SlotId",
                table: "ProfundaEinschreibungen",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundaEinschreibungen",
                table: "ProfundaEinschreibungen",
                columns: new[] { "BetroffenePersonId", "ProfundumInstanzId", "SlotId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProfundaEinschreibungen_SlotId",
                table: "ProfundaEinschreibungen",
                column: "SlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaEinschreibungen_ProfundaSlots_SlotId",
                table: "ProfundaEinschreibungen",
                column: "SlotId",
                principalTable: "ProfundaSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaEinschreibungen_ProfundaSlots_SlotId",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundaEinschreibungen",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropIndex(
                name: "IX_ProfundaEinschreibungen_SlotId",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropColumn(
                name: "SlotId",
                table: "ProfundaEinschreibungen");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundaEinschreibungen",
                table: "ProfundaEinschreibungen",
                columns: new[] { "BetroffenePersonId", "ProfundumInstanzId" });
        }
    }
}
