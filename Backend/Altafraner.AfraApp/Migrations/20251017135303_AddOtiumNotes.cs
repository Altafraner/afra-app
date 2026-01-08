using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOtiumNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OtiaEinschreibungsNotizen",
                columns: table => new
                {
                    BlockId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiaEinschreibungsNotizen", x => new { x.BlockId, x.StudentId, x.AuthorId });
                    table.ForeignKey(
                        name: "FK_OtiaEinschreibungsNotizen_Blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtiaEinschreibungsNotizen_Personen_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtiaEinschreibungsNotizen_Personen_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OtiaEinschreibungsNotizen_AuthorId",
                table: "OtiaEinschreibungsNotizen",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaEinschreibungsNotizen_StudentId",
                table: "OtiaEinschreibungsNotizen",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtiaEinschreibungsNotizen");
        }
    }
}
