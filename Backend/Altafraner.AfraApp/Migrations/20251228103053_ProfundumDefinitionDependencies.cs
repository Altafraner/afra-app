using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class ProfundumDefinitionDependencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfundaInstanzDependencies");

            migrationBuilder.CreateTable(
                name: "ProfundumDefinitionDependencies",
                columns: table => new
                {
                    DependencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    DependantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfundumDefinitionDependencies", x => new { x.DependencyId, x.DependantId });
                    table.ForeignKey(
                        name: "FK_ProfundumDefinitionDependencies_Profunda_DependantId",
                        column: x => x.DependantId,
                        principalTable: "Profunda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfundumDefinitionDependencies_Profunda_DependencyId",
                        column: x => x.DependencyId,
                        principalTable: "Profunda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfundumDefinitionDependencies_DependantId",
                table: "ProfundumDefinitionDependencies",
                column: "DependantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfundumDefinitionDependencies");

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
                name: "IX_ProfundaInstanzDependencies_DependantId",
                table: "ProfundaInstanzDependencies",
                column: "DependantId");
        }
    }
}
