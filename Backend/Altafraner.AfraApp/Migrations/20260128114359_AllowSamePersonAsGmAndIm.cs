using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AllowSamePersonAsGmAndIm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_mentor_mentee_relations",
                table: "mentor_mentee_relations");

            migrationBuilder.AddPrimaryKey(
                name: "pk_mentor_mentee_relations",
                table: "mentor_mentee_relations",
                columns: new[] { "mentor_id", "student_id", "type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_mentor_mentee_relations",
                table: "mentor_mentee_relations");

            migrationBuilder.AddPrimaryKey(
                name: "pk_mentor_mentee_relations",
                table: "mentor_mentee_relations",
                columns: new[] { "mentor_id", "student_id" });
        }
    }
}
