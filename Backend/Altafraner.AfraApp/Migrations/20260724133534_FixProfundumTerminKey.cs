using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class FixProfundumTerminKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda_termine",
                table: "profunda_termine");

            migrationBuilder.DropIndex(
                name: "ix_profunda_termine_slot_id",
                table: "profunda_termine");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda_termine",
                table: "profunda_termine",
                columns: new[] { "slot_id", "day" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda_termine",
                table: "profunda_termine");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda_termine",
                table: "profunda_termine",
                column: "day");

            migrationBuilder.CreateIndex(
                name: "ix_profunda_termine_slot_id",
                table: "profunda_termine",
                column: "slot_id");
        }
    }
}
