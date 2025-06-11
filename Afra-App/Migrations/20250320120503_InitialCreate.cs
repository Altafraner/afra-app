using System;
using Afra_App.Otium.Domain.Models.Schuljahr;
using Afra_App.User.Domain.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:person_rolle", "student,tutor")
                .Annotation("Npgsql:Enum:wochentyp", "h,n");

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtiaKategorien",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Bezeichnung = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CssColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiaKategorien", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtiaKategorien_OtiaKategorien_ParentId",
                        column: x => x.ParentId,
                        principalTable: "OtiaKategorien",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Personen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Vorname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nachname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MentorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Rolle = table.Column<Rolle>(type: "person_rolle", nullable: false)
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
                name: "Schultage",
                columns: table => new
                {
                    Datum = table.Column<DateOnly>(type: "date", nullable: false),
                    Wochentyp = table.Column<Wochentyp>(type: "wochentyp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schultage", x => x.Datum);
                });

            migrationBuilder.CreateTable(
                name: "Otia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Bezeichnung = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Beschreibung = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    KategorieId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Otia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Otia_OtiaKategorien_KategorieId",
                        column: x => x.KategorieId,
                        principalTable: "OtiaKategorien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledEmails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledEmails_Personen_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Blocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchultagKey = table.Column<DateOnly>(type: "date", nullable: false),
                    Nummer = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blocks_Schultage_SchultagKey",
                        column: x => x.SchultagKey,
                        principalTable: "Schultage",
                        principalColumn: "Datum",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtiaWiederholungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Wochentag = table.Column<int>(type: "integer", nullable: false),
                    Wochentyp = table.Column<Wochentyp>(type: "wochentyp", nullable: false),
                    Block = table.Column<short>(type: "smallint", nullable: false),
                    OtiumId = table.Column<Guid>(type: "uuid", nullable: false),
                    TutorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Ort = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
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
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OtiumPerson",
                columns: table => new
                {
                    VerantwortlicheId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerwalteteOtiaId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WiederholungId = table.Column<Guid>(type: "uuid", nullable: true),
                    BlockId = table.Column<Guid>(type: "uuid", nullable: false),
                    IstAbgesagt = table.Column<bool>(type: "boolean", nullable: false),
                    MaxEinschreibungen = table.Column<int>(type: "integer", nullable: true),
                    OtiumId = table.Column<Guid>(type: "uuid", nullable: false),
                    TutorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Ort = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiaTermine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtiaTermine_Blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OtiaEinschreibungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TerminId = table.Column<Guid>(type: "uuid", nullable: false),
                    BetroffenePersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Interval_Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Interval_Start = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
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
                name: "IX_Blocks_SchultagKey_Nummer",
                table: "Blocks",
                columns: new[] { "SchultagKey", "Nummer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Otia_KategorieId",
                table: "Otia",
                column: "KategorieId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaEinschreibungen_BetroffenePersonId",
                table: "OtiaEinschreibungen",
                column: "BetroffenePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaEinschreibungen_TerminId",
                table: "OtiaEinschreibungen",
                column: "TerminId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaKategorien_ParentId",
                table: "OtiaKategorien",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaTermine_BlockId",
                table: "OtiaTermine",
                column: "BlockId");

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
                name: "IX_ScheduledEmails_RecipientId",
                table: "ScheduledEmails",
                column: "RecipientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "OtiaEinschreibungen");

            migrationBuilder.DropTable(
                name: "OtiumPerson");

            migrationBuilder.DropTable(
                name: "ScheduledEmails");

            migrationBuilder.DropTable(
                name: "OtiaTermine");

            migrationBuilder.DropTable(
                name: "Blocks");

            migrationBuilder.DropTable(
                name: "OtiaWiederholungen");

            migrationBuilder.DropTable(
                name: "Schultage");

            migrationBuilder.DropTable(
                name: "Otia");

            migrationBuilder.DropTable(
                name: "Personen");

            migrationBuilder.DropTable(
                name: "OtiaKategorien");
        }
    }
}
