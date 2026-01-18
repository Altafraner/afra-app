using System.Collections.Generic;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class ScopeOtiumCategoryRuleToWeektype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Required",
                table: "OtiaKategorien");

            migrationBuilder.AddColumn<List<Wochentyp>>(
                name: "RequiredIn",
                table: "OtiaKategorien",
                type: "wochentyp[]",
                defaultValue: new List<Wochentyp>(),
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredIn",
                table: "OtiaKategorien");

            migrationBuilder.AddColumn<bool>(
                name: "Required",
                table: "OtiaKategorien",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
