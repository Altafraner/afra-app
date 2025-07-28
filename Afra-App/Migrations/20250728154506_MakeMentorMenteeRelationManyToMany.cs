using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class MakeMentorMenteeRelationManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personen_Personen_MentorId",
                table: "Personen");

            migrationBuilder.DropIndex(
                name: "IX_Personen_MentorId",
                table: "Personen");

            migrationBuilder.DropColumn(
                name: "MentorId",
                table: "Personen");

            migrationBuilder.CreateTable(
                name: "MentorMenteeRelations",
                columns: table => new
                {
                    MentorId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorMenteeRelations", x => new { x.MentorId, x.StudentId });
                    table.ForeignKey(
                        name: "FK_MentorMenteeRelations_Personen_MentorId",
                        column: x => x.MentorId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentorMenteeRelations_Personen_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MentorMenteeRelations_StudentId",
                table: "MentorMenteeRelations",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MentorMenteeRelations");

            migrationBuilder.AddColumn<Guid>(
                name: "MentorId",
                table: "Personen",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personen_MentorId",
                table: "Personen",
                column: "MentorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Personen_Personen_MentorId",
                table: "Personen",
                column: "MentorId",
                principalTable: "Personen",
                principalColumn: "Id");
        }
    }
}
