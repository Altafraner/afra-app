using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonAndIdToProfundumTermin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_profundum_instanz_profundum_slot_profunda_instanzen_profund~",
                table: "profundum_instanz_profundum_slot");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda_termine",
                table: "profunda_termine");

            migrationBuilder.RenameColumn(
                name: "profundum_instanz_id",
                table: "profundum_instanz_profundum_slot",
                newName: "angebote_id");

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "profunda_termine",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("UPDATE profunda_termine SET id = uuidv7() WHERE id IS NULL");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "profunda_termine",
                type: "uuid",
                nullable: false,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "lesson",
                table: "profunda_termine",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda_termine",
                table: "profunda_termine",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_profunda_termine_day",
                table: "profunda_termine",
                column: "day",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_instanz_profundum_slot_profunda_instanzen_angebot~",
                table: "profundum_instanz_profundum_slot",
                column: "angebote_id",
                principalTable: "profunda_instanzen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_profundum_instanz_profundum_slot_profunda_instanzen_angebot~",
                table: "profundum_instanz_profundum_slot");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda_termine",
                table: "profunda_termine");

            migrationBuilder.DropIndex(
                name: "ix_profunda_termine_day",
                table: "profunda_termine");

            migrationBuilder.DropColumn(
                name: "id",
                table: "profunda_termine");

            migrationBuilder.DropColumn(
                name: "lesson",
                table: "profunda_termine");

            migrationBuilder.RenameColumn(
                name: "angebote_id",
                table: "profundum_instanz_profundum_slot",
                newName: "profundum_instanz_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda_termine",
                table: "profunda_termine",
                column: "day");

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_instanz_profundum_slot_profunda_instanzen_profund~",
                table: "profundum_instanz_profundum_slot",
                column: "profundum_instanz_id",
                principalTable: "profunda_instanzen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
