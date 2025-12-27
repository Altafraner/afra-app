using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProfundumDependenciesVerantwortliche : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "ProfundaEinschreibungen",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Beschreibung",
                table: "Profunda",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PersonProfundumDefinition",
                columns: table => new
                {
                    BetreuteProfundaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerantwortlicheId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonProfundumDefinition", x => new { x.BetreuteProfundaId, x.VerantwortlicheId });
                    table.ForeignKey(
                        name: "FK_PersonProfundumDefinition_Personen_VerantwortlicheId",
                        column: x => x.VerantwortlicheId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonProfundumDefinition_Profunda_BetreuteProfundaId",
                        column: x => x.BetreuteProfundaId,
                        principalTable: "Profunda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfundaInstanzDependencies",
                columns: table => new
                {
                    DependencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    DependantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundaInstanzDependencies", x => new { x.DependencyId, x.DependantId });
                    table.ForeignKey(
                        name: "FK_ProfundaInstanzDependencies_ProfundaInstanzen_DependantId",
                        column: x => x.DependantId,
                        principalTable: "ProfundaInstanzen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfundaInstanzDependencies_ProfundaInstanzen_DependencyId",
                        column: x => x.DependencyId,
                        principalTable: "ProfundaInstanzen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfundumDefinition_VerantwortlicheId",
                table: "PersonProfundumDefinition",
                column: "VerantwortlicheId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfundaInstanzDependencies_DependantId",
                table: "ProfundaInstanzDependencies",
                column: "DependantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonProfundumDefinition");

            migrationBuilder.DropTable(
                name: "ProfundaInstanzDependencies");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropColumn(
                name: "Beschreibung",
                table: "Profunda");
        }
    }
}
