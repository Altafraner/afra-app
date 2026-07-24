using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerwahlToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_profundum_partner_einladungen_einwahl_zeitraum_id",
                table: "profundum_partner_einladungen");

            migrationBuilder.AddColumn<string>(
                name: "token",
                table: "profundum_partner_einladungen",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_profundum_partner_einladungen_einwahl_zeitraum_id_token",
                table: "profundum_partner_einladungen",
                columns: new[] { "einwahl_zeitraum_id", "token" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_profundum_partner_einladungen_einwahl_zeitraum_id_token",
                table: "profundum_partner_einladungen");

            migrationBuilder.DropColumn(
                name: "token",
                table: "profundum_partner_einladungen");

            migrationBuilder.CreateIndex(
                name: "ix_profundum_partner_einladungen_einwahl_zeitraum_id",
                table: "profundum_partner_einladungen",
                column: "einwahl_zeitraum_id");
        }
    }
}
