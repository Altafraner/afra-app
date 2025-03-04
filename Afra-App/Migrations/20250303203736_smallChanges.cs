using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class smallChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OtiaKategorien_Otia_OtiumId",
                table: "OtiaKategorien");

            migrationBuilder.DropIndex(
                name: "IX_OtiaKategorien_OtiumId",
                table: "OtiaKategorien");

            migrationBuilder.DropColumn(
                name: "OtiumId",
                table: "OtiaKategorien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
