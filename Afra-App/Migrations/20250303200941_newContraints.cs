using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class newContraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxEinschreibungen",
                table: "OtiaTermine",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OtiumId",
                table: "OtiaKategorien",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OtiaKategorien_OtiumId",
                table: "OtiaKategorien",
                column: "OtiumId");

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaKategorien_Otia_OtiumId",
                table: "OtiaKategorien",
                column: "OtiumId",
                principalTable: "Otia",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OtiaKategorien_Otia_OtiumId",
                table: "OtiaKategorien");

            migrationBuilder.DropIndex(
                name: "IX_OtiaKategorien_OtiumId",
                table: "OtiaKategorien");

            migrationBuilder.DropColumn(
                name: "MaxEinschreibungen",
                table: "OtiaTermine");

            migrationBuilder.DropColumn(
                name: "OtiumId",
                table: "OtiaKategorien");
        }
    }
}
