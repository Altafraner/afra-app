using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class ProfundumEnrollmentNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaEinschreibungen_ProfundaInstanzen_ProfundumInstanzId",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundaEinschreibungen",
                table: "ProfundaEinschreibungen");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProfundumInstanzId",
                table: "ProfundaEinschreibungen",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundaEinschreibungen",
                table: "ProfundaEinschreibungen",
                columns: new[] { "BetroffenePersonId", "SlotId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaEinschreibungen_ProfundaInstanzen_ProfundumInstanzId",
                table: "ProfundaEinschreibungen",
                column: "ProfundumInstanzId",
                principalTable: "ProfundaInstanzen",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaEinschreibungen_ProfundaInstanzen_ProfundumInstanzId",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundaEinschreibungen",
                table: "ProfundaEinschreibungen");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProfundumInstanzId",
                table: "ProfundaEinschreibungen",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundaEinschreibungen",
                table: "ProfundaEinschreibungen",
                columns: new[] { "BetroffenePersonId", "ProfundumInstanzId", "SlotId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaEinschreibungen_ProfundaInstanzen_ProfundumInstanzId",
                table: "ProfundaEinschreibungen",
                column: "ProfundumInstanzId",
                principalTable: "ProfundaInstanzen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
