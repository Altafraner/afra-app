using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class Keys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaBelegWuensche_Personen_BetroffenePersonKey",
                table: "ProfundaBelegWuensche");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaBelegWuensche_ProfundaInstanzen_ProfundumInstanzKey",
                table: "ProfundaBelegWuensche");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaEinschreibungen_Personen_BetroffenePersonKey",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaEinschreibungen_ProfundaInstanzen_ProfundumInstanzK~",
                table: "ProfundaEinschreibungen");

            migrationBuilder.RenameColumn(
                name: "ProfundumInstanzKey",
                table: "ProfundaEinschreibungen",
                newName: "ProfundumInstanzId");

            migrationBuilder.RenameColumn(
                name: "BetroffenePersonKey",
                table: "ProfundaEinschreibungen",
                newName: "BetroffenePersonId");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaEinschreibungen_ProfundumInstanzKey",
                table: "ProfundaEinschreibungen",
                newName: "IX_ProfundaEinschreibungen_ProfundumInstanzId");

            migrationBuilder.RenameColumn(
                name: "BetroffenePersonKey",
                table: "ProfundaBelegWuensche",
                newName: "BetroffenePersonId");

            migrationBuilder.RenameColumn(
                name: "ProfundumInstanzKey",
                table: "ProfundaBelegWuensche",
                newName: "ProfundumInstanzId");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaBelegWuensche_BetroffenePersonKey",
                table: "ProfundaBelegWuensche",
                newName: "IX_ProfundaBelegWuensche_BetroffenePersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaBelegWuensche_Personen_BetroffenePersonId",
                table: "ProfundaBelegWuensche",
                column: "BetroffenePersonId",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaBelegWuensche_ProfundaInstanzen_ProfundumInstanzId",
                table: "ProfundaBelegWuensche",
                column: "ProfundumInstanzId",
                principalTable: "ProfundaInstanzen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaEinschreibungen_Personen_BetroffenePersonId",
                table: "ProfundaEinschreibungen",
                column: "BetroffenePersonId",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaEinschreibungen_ProfundaInstanzen_ProfundumInstanzId",
                table: "ProfundaEinschreibungen",
                column: "ProfundumInstanzId",
                principalTable: "ProfundaInstanzen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaBelegWuensche_Personen_BetroffenePersonId",
                table: "ProfundaBelegWuensche");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaBelegWuensche_ProfundaInstanzen_ProfundumInstanzId",
                table: "ProfundaBelegWuensche");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaEinschreibungen_Personen_BetroffenePersonId",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaEinschreibungen_ProfundaInstanzen_ProfundumInstanzId",
                table: "ProfundaEinschreibungen");

            migrationBuilder.RenameColumn(
                name: "ProfundumInstanzId",
                table: "ProfundaEinschreibungen",
                newName: "ProfundumInstanzKey");

            migrationBuilder.RenameColumn(
                name: "BetroffenePersonId",
                table: "ProfundaEinschreibungen",
                newName: "BetroffenePersonKey");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaEinschreibungen_ProfundumInstanzId",
                table: "ProfundaEinschreibungen",
                newName: "IX_ProfundaEinschreibungen_ProfundumInstanzKey");

            migrationBuilder.RenameColumn(
                name: "BetroffenePersonId",
                table: "ProfundaBelegWuensche",
                newName: "BetroffenePersonKey");

            migrationBuilder.RenameColumn(
                name: "ProfundumInstanzId",
                table: "ProfundaBelegWuensche",
                newName: "ProfundumInstanzKey");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaBelegWuensche_BetroffenePersonId",
                table: "ProfundaBelegWuensche",
                newName: "IX_ProfundaBelegWuensche_BetroffenePersonKey");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaBelegWuensche_Personen_BetroffenePersonKey",
                table: "ProfundaBelegWuensche",
                column: "BetroffenePersonKey",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaBelegWuensche_ProfundaInstanzen_ProfundumInstanzKey",
                table: "ProfundaBelegWuensche",
                column: "ProfundumInstanzKey",
                principalTable: "ProfundaInstanzen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaEinschreibungen_Personen_BetroffenePersonKey",
                table: "ProfundaEinschreibungen",
                column: "BetroffenePersonKey",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaEinschreibungen_ProfundaInstanzen_ProfundumInstanzK~",
                table: "ProfundaEinschreibungen",
                column: "ProfundumInstanzKey",
                principalTable: "ProfundaInstanzen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
