using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProfundumUserAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "ProfundaInstanzen",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedById",
                table: "ProfundaInstanzen",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "ProfundaEinschreibungen",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedById",
                table: "ProfundaEinschreibungen",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Profunda",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedById",
                table: "Profunda",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "ProfundaInstanzen");

            migrationBuilder.DropColumn(
                name: "LastModifiedById",
                table: "ProfundaInstanzen");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropColumn(
                name: "LastModifiedById",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Profunda");

            migrationBuilder.DropColumn(
                name: "LastModifiedById",
                table: "Profunda");
        }
    }
}
