using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class SetBrokenModifiedTimesAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Otia");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Otia");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Otia",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Otia",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "OtiaTermine");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "OtiaTermine");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "OtiaEinschreibungen");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "OtiaEinschreibungen");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "OtiaTermine",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "OtiaTermine",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "OtiaEinschreibungen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "OtiaEinschreibungen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
