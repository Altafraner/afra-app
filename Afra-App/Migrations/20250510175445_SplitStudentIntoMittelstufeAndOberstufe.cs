using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class SplitStudentIntoMittelstufeAndOberstufe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 ALTER TYPE person_rolle ADD VALUE IF NOT EXISTS 'oberstufe' AFTER 'student';
                                 ALTER TYPE person_rolle RENAME VALUE 'student' TO 'mittelstufe';
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 ALTER TYPE person_rolle RENAME VALUE 'mittelstufe' TO 'student';
                                 """);
        }
    }
}
