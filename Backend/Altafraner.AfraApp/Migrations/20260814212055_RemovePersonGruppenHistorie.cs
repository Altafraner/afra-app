using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class RemovePersonGruppenHistorie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "person_gruppen_historien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "person_gruppen_historien",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gruppe = table.Column<string>(type: "text", nullable: true),
                    gueltig_ab = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_person_gruppen_historien", x => x.id);
                    table.ForeignKey(
                        name: "fk_person_gruppen_historien_personen_person_id",
                        column: x => x.person_id,
                        principalTable: "personen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_person_gruppen_historien_person_id_gueltig_ab",
                table: "person_gruppen_historien",
                columns: new[] { "person_id", "gueltig_ab" });
        }
    }
}
