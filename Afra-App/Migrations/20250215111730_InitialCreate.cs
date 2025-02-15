using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OtiaKategorien",
                columns: table => new
                {
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiaKategorien", x => x.Bezeichnung);
                });

            migrationBuilder.CreateTable(
                name: "Personen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Vorname = table.Column<string>(type: "TEXT", nullable: false),
                    Nachname = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    MentorId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Personen_Personen_MentorId",
                        column: x => x.MentorId,
                        principalTable: "Personen",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Rollen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Permissions = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rollen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Otia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Beschreibung = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    KategorieBezeichnung = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Otia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Otia_OtiaKategorien_KategorieBezeichnung",
                        column: x => x.KategorieBezeichnung,
                        principalTable: "OtiaKategorien",
                        principalColumn: "Bezeichnung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonRolle",
                columns: table => new
                {
                    PersonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RollenId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonRolle", x => new { x.PersonId, x.RollenId });
                    table.ForeignKey(
                        name: "FK_PersonRolle_Personen_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonRolle_Rollen_RollenId",
                        column: x => x.RollenId,
                        principalTable: "Rollen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtiaWiederholungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Wochentag = table.Column<int>(type: "INTEGER", nullable: false),
                    OtiumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Block = table.Column<byte>(type: "INTEGER", nullable: false),
                    Ort = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiaWiederholungen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtiaWiederholungen_Otia_OtiumId",
                        column: x => x.OtiumId,
                        principalTable: "Otia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtiaWiederholungen_Personen_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtiumPerson",
                columns: table => new
                {
                    VerantwortlicheId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VerwalteteOtiaId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiumPerson", x => new { x.VerantwortlicheId, x.VerwalteteOtiaId });
                    table.ForeignKey(
                        name: "FK_OtiumPerson_Otia_VerwalteteOtiaId",
                        column: x => x.VerwalteteOtiaId,
                        principalTable: "Otia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtiumPerson_Personen_VerantwortlicheId",
                        column: x => x.VerantwortlicheId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtiaTermine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Datum = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    WiederholungId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IstAbgesagt = table.Column<bool>(type: "INTEGER", nullable: false),
                    OtiumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Block = table.Column<byte>(type: "INTEGER", nullable: false),
                    Ort = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiaTermine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtiaTermine_OtiaWiederholungen_WiederholungId",
                        column: x => x.WiederholungId,
                        principalTable: "OtiaWiederholungen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OtiaTermine_Otia_OtiumId",
                        column: x => x.OtiumId,
                        principalTable: "Otia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtiaTermine_Personen_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtiaEinschreibungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TerminId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BetroffenePersonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Start = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    Ende = table.Column<TimeOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiaEinschreibungen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtiaEinschreibungen_OtiaTermine_TerminId",
                        column: x => x.TerminId,
                        principalTable: "OtiaTermine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtiaEinschreibungen_Personen_BetroffenePersonId",
                        column: x => x.BetroffenePersonId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Otia_KategorieBezeichnung",
                table: "Otia",
                column: "KategorieBezeichnung");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaEinschreibungen_BetroffenePersonId",
                table: "OtiaEinschreibungen",
                column: "BetroffenePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaEinschreibungen_TerminId",
                table: "OtiaEinschreibungen",
                column: "TerminId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaTermine_OtiumId",
                table: "OtiaTermine",
                column: "OtiumId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaTermine_TutorId",
                table: "OtiaTermine",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaTermine_WiederholungId",
                table: "OtiaTermine",
                column: "WiederholungId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaWiederholungen_OtiumId",
                table: "OtiaWiederholungen",
                column: "OtiumId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaWiederholungen_TutorId",
                table: "OtiaWiederholungen",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiumPerson_VerwalteteOtiaId",
                table: "OtiumPerson",
                column: "VerwalteteOtiaId");

            migrationBuilder.CreateIndex(
                name: "IX_Personen_MentorId",
                table: "Personen",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonRolle_RollenId",
                table: "PersonRolle",
                column: "RollenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtiaEinschreibungen");

            migrationBuilder.DropTable(
                name: "OtiumPerson");

            migrationBuilder.DropTable(
                name: "PersonRolle");

            migrationBuilder.DropTable(
                name: "OtiaTermine");

            migrationBuilder.DropTable(
                name: "Rollen");

            migrationBuilder.DropTable(
                name: "OtiaWiederholungen");

            migrationBuilder.DropTable(
                name: "Otia");

            migrationBuilder.DropTable(
                name: "Personen");

            migrationBuilder.DropTable(
                name: "OtiaKategorien");
        }
    }
}
