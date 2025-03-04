using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class KategoriesAsTree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Otia_OtiaKategorien_KategorieBezeichnung",
                table: "Otia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtiaKategorien",
                table: "OtiaKategorien");

            migrationBuilder.DropIndex(
                name: "IX_Otia_KategorieBezeichnung",
                table: "Otia");

            migrationBuilder.DropColumn(
                name: "KategorieBezeichnung",
                table: "Otia");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "OtiaKategorien",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "OtiaKategorien",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KategorieId",
                table: "Otia",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtiaKategorien",
                table: "OtiaKategorien",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaKategorien_ParentId",
                table: "OtiaKategorien",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Otia_KategorieId",
                table: "Otia",
                column: "KategorieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Otia_OtiaKategorien_KategorieId",
                table: "Otia",
                column: "KategorieId",
                principalTable: "OtiaKategorien",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaKategorien_OtiaKategorien_ParentId",
                table: "OtiaKategorien",
                column: "ParentId",
                principalTable: "OtiaKategorien",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Otia_OtiaKategorien_KategorieId",
                table: "Otia");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaKategorien_OtiaKategorien_ParentId",
                table: "OtiaKategorien");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtiaKategorien",
                table: "OtiaKategorien");

            migrationBuilder.DropIndex(
                name: "IX_OtiaKategorien_ParentId",
                table: "OtiaKategorien");

            migrationBuilder.DropIndex(
                name: "IX_Otia_KategorieId",
                table: "Otia");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "OtiaKategorien");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "OtiaKategorien");

            migrationBuilder.DropColumn(
                name: "KategorieId",
                table: "Otia");

            migrationBuilder.AddColumn<string>(
                name: "KategorieBezeichnung",
                table: "Otia",
                type: "character varying(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtiaKategorien",
                table: "OtiaKategorien",
                column: "Bezeichnung");

            migrationBuilder.CreateIndex(
                name: "IX_Otia_KategorieBezeichnung",
                table: "Otia",
                column: "KategorieBezeichnung");

            migrationBuilder.AddForeignKey(
                name: "FK_Otia_OtiaKategorien_KategorieBezeichnung",
                table: "Otia",
                column: "KategorieBezeichnung",
                principalTable: "OtiaKategorien",
                principalColumn: "Bezeichnung",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
