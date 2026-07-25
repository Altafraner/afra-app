using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOtiumBlockSupervisors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "otium_block_supervisor",
                columns: table => new
                {
                    block_id = table.Column<Guid>(type: "uuid", nullable: false),
                    supervisors_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_otium_block_supervisor", x => new { x.block_id, x.supervisors_id });
                    table.ForeignKey(
                        name: "fk_otium_block_supervisor_blocks_block_id",
                        column: x => x.block_id,
                        principalTable: "blocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_otium_block_supervisor_personen_supervisors_id",
                        column: x => x.supervisors_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_otium_block_supervisor_supervisors_id",
                table: "otium_block_supervisor",
                column: "supervisors_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "otium_block_supervisor");
        }
    }
}
