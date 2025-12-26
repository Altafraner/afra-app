using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AllLowerCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_Schultage_SchultagKey",
                table: "Blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_CalendarSubscriptions_Personen_BetroffenePersonId",
                table: "CalendarSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_MentorMenteeRelations_Personen_MentorId",
                table: "MentorMenteeRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_MentorMenteeRelations_Personen_StudentId",
                table: "MentorMenteeRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_Otia_OtiaKategorien_KategorieId",
                table: "Otia");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaAnwesenheiten_Blocks_BlockId",
                table: "OtiaAnwesenheiten");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaAnwesenheiten_Personen_StudentId",
                table: "OtiaAnwesenheiten");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaEinschreibungen_OtiaTermine_TerminId",
                table: "OtiaEinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaEinschreibungen_Personen_BetroffenePersonId",
                table: "OtiaEinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaEinschreibungsNotizen_Blocks_BlockId",
                table: "OtiaEinschreibungsNotizen");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaEinschreibungsNotizen_Personen_AuthorId",
                table: "OtiaEinschreibungsNotizen");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaEinschreibungsNotizen_Personen_StudentId",
                table: "OtiaEinschreibungsNotizen");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaKategorien_OtiaKategorien_ParentId",
                table: "OtiaKategorien");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaTermine_Blocks_BlockId",
                table: "OtiaTermine");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaTermine_OtiaWiederholungen_WiederholungId",
                table: "OtiaTermine");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaTermine_Otia_OtiumId",
                table: "OtiaTermine");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaTermine_Personen_TutorId",
                table: "OtiaTermine");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaWiederholungen_Otia_OtiumId",
                table: "OtiaWiederholungen");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiaWiederholungen_Personen_TutorId",
                table: "OtiaWiederholungen");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiumDefinitionPerson_Otia_VerwalteteOtiaId",
                table: "OtiumDefinitionPerson");

            migrationBuilder.DropForeignKey(
                name: "FK_OtiumDefinitionPerson_Personen_VerantwortlicheId",
                table: "OtiumDefinitionPerson");

            migrationBuilder.DropForeignKey(
                name: "FK_Profunda_ProfundaKategorien_KategorieId",
                table: "Profunda");

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

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaInstanzen_Profunda_ProfundumId",
                table: "ProfundaInstanzen");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaSlots_ProfundumEinwahlZeitraeume_EinwahlZeitraumId",
                table: "ProfundaSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumInstanzProfundumSlot_ProfundaInstanzen_ProfundumIn~",
                table: "ProfundumInstanzProfundumSlot");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumInstanzProfundumSlot_ProfundaSlots_SlotsId",
                table: "ProfundumInstanzProfundumSlot");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledEmails_Personen_RecipientId",
                table: "ScheduledEmails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Schultage",
                table: "Schultage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduledEmails",
                table: "ScheduledEmails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundumInstanzProfundumSlot",
                table: "ProfundumInstanzProfundumSlot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundumEinwahlZeitraeume",
                table: "ProfundumEinwahlZeitraeume");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundaSlots",
                table: "ProfundaSlots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundaKategorien",
                table: "ProfundaKategorien");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundaInstanzen",
                table: "ProfundaInstanzen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundaEinschreibungen",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundaBelegWuensche",
                table: "ProfundaBelegWuensche");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Profunda",
                table: "Profunda");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Personen",
                table: "Personen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtiumDefinitionPerson",
                table: "OtiumDefinitionPerson");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtiaWiederholungen",
                table: "OtiaWiederholungen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtiaTermine",
                table: "OtiaTermine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtiaKategorien",
                table: "OtiaKategorien");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtiaEinschreibungsNotizen",
                table: "OtiaEinschreibungsNotizen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtiaEinschreibungen",
                table: "OtiaEinschreibungen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtiaAnwesenheiten",
                table: "OtiaAnwesenheiten");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Otia",
                table: "Otia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MentorMenteeRelations",
                table: "MentorMenteeRelations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataProtectionKeys",
                table: "DataProtectionKeys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CalendarSubscriptions",
                table: "CalendarSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Blocks",
                table: "Blocks");

            migrationBuilder.RenameTable(
                name: "Schultage",
                newName: "schultage");

            migrationBuilder.RenameTable(
                name: "ScheduledEmails",
                newName: "scheduledemails");

            migrationBuilder.RenameTable(
                name: "ProfundumInstanzProfundumSlot",
                newName: "profunduminstanzprofundumslot");

            migrationBuilder.RenameTable(
                name: "ProfundumEinwahlZeitraeume",
                newName: "profundumeinwahlzeitraeume");

            migrationBuilder.RenameTable(
                name: "ProfundaSlots",
                newName: "profundaslots");

            migrationBuilder.RenameTable(
                name: "ProfundaKategorien",
                newName: "profundakategorien");

            migrationBuilder.RenameTable(
                name: "ProfundaInstanzen",
                newName: "profundainstanzen");

            migrationBuilder.RenameTable(
                name: "ProfundaEinschreibungen",
                newName: "profundaeinschreibungen");

            migrationBuilder.RenameTable(
                name: "ProfundaBelegWuensche",
                newName: "profundabelegwuensche");

            migrationBuilder.RenameTable(
                name: "Profunda",
                newName: "profunda");

            migrationBuilder.RenameTable(
                name: "Personen",
                newName: "personen");

            migrationBuilder.RenameTable(
                name: "OtiumDefinitionPerson",
                newName: "otiumdefinitionperson");

            migrationBuilder.RenameTable(
                name: "OtiaWiederholungen",
                newName: "otiawiederholungen");

            migrationBuilder.RenameTable(
                name: "OtiaTermine",
                newName: "otiatermine");

            migrationBuilder.RenameTable(
                name: "OtiaKategorien",
                newName: "otiakategorien");

            migrationBuilder.RenameTable(
                name: "OtiaEinschreibungsNotizen",
                newName: "otiaeinschreibungsnotizen");

            migrationBuilder.RenameTable(
                name: "OtiaEinschreibungen",
                newName: "otiaeinschreibungen");

            migrationBuilder.RenameTable(
                name: "OtiaAnwesenheiten",
                newName: "otiaanwesenheiten");

            migrationBuilder.RenameTable(
                name: "Otia",
                newName: "otia");

            migrationBuilder.RenameTable(
                name: "MentorMenteeRelations",
                newName: "mentormenteerelations");

            migrationBuilder.RenameTable(
                name: "DataProtectionKeys",
                newName: "dataprotectionkeys");

            migrationBuilder.RenameTable(
                name: "CalendarSubscriptions",
                newName: "calendarsubscriptions");

            migrationBuilder.RenameTable(
                name: "Blocks",
                newName: "blocks");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduledEmails_RecipientId",
                table: "scheduledemails",
                newName: "IX_scheduledemails_RecipientId");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundumInstanzProfundumSlot_SlotsId",
                table: "profunduminstanzprofundumslot",
                newName: "IX_profunduminstanzprofundumslot_SlotsId");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaSlots_EinwahlZeitraumId",
                table: "profundaslots",
                newName: "IX_profundaslots_EinwahlZeitraumId");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaInstanzen_ProfundumId",
                table: "profundainstanzen",
                newName: "IX_profundainstanzen_ProfundumId");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaEinschreibungen_ProfundumInstanzId",
                table: "profundaeinschreibungen",
                newName: "IX_profundaeinschreibungen_ProfundumInstanzId");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaBelegWuensche_BetroffenePersonId",
                table: "profundabelegwuensche",
                newName: "IX_profundabelegwuensche_BetroffenePersonId");

            migrationBuilder.RenameIndex(
                name: "IX_Profunda_KategorieId",
                table: "profunda",
                newName: "IX_profunda_KategorieId");

            migrationBuilder.RenameIndex(
                name: "IX_OtiumDefinitionPerson_VerwalteteOtiaId",
                table: "otiumdefinitionperson",
                newName: "IX_otiumdefinitionperson_VerwalteteOtiaId");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaWiederholungen_TutorId",
                table: "otiawiederholungen",
                newName: "IX_otiawiederholungen_TutorId");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaWiederholungen_OtiumId",
                table: "otiawiederholungen",
                newName: "IX_otiawiederholungen_OtiumId");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaTermine_WiederholungId",
                table: "otiatermine",
                newName: "IX_otiatermine_WiederholungId");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaTermine_TutorId",
                table: "otiatermine",
                newName: "IX_otiatermine_TutorId");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaTermine_OtiumId",
                table: "otiatermine",
                newName: "IX_otiatermine_OtiumId");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaTermine_BlockId",
                table: "otiatermine",
                newName: "IX_otiatermine_BlockId");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaKategorien_ParentId",
                table: "otiakategorien",
                newName: "IX_otiakategorien_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaEinschreibungsNotizen_StudentId",
                table: "otiaeinschreibungsnotizen",
                newName: "IX_otiaeinschreibungsnotizen_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaEinschreibungsNotizen_AuthorId",
                table: "otiaeinschreibungsnotizen",
                newName: "IX_otiaeinschreibungsnotizen_AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaEinschreibungen_TerminId",
                table: "otiaeinschreibungen",
                newName: "IX_otiaeinschreibungen_TerminId");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaEinschreibungen_BetroffenePersonId",
                table: "otiaeinschreibungen",
                newName: "IX_otiaeinschreibungen_BetroffenePersonId");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaAnwesenheiten_StudentId",
                table: "otiaanwesenheiten",
                newName: "IX_otiaanwesenheiten_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Otia_KategorieId",
                table: "otia",
                newName: "IX_otia_KategorieId");

            migrationBuilder.RenameIndex(
                name: "IX_MentorMenteeRelations_StudentId",
                table: "mentormenteerelations",
                newName: "IX_mentormenteerelations_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarSubscriptions_BetroffenePersonId",
                table: "calendarsubscriptions",
                newName: "IX_calendarsubscriptions_BetroffenePersonId");

            migrationBuilder.RenameIndex(
                name: "IX_Blocks_SchultagKey_SchemaId",
                table: "blocks",
                newName: "IX_blocks_SchultagKey_SchemaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_schultage",
                table: "schultage",
                column: "Datum");

            migrationBuilder.AddPrimaryKey(
                name: "PK_scheduledemails",
                table: "scheduledemails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_profunduminstanzprofundumslot",
                table: "profunduminstanzprofundumslot",
                columns: new[] { "ProfundumInstanzId", "SlotsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_profundumeinwahlzeitraeume",
                table: "profundumeinwahlzeitraeume",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_profundaslots",
                table: "profundaslots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_profundakategorien",
                table: "profundakategorien",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_profundainstanzen",
                table: "profundainstanzen",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_profundaeinschreibungen",
                table: "profundaeinschreibungen",
                columns: new[] { "BetroffenePersonId", "ProfundumInstanzId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_profundabelegwuensche",
                table: "profundabelegwuensche",
                columns: new[] { "ProfundumInstanzId", "BetroffenePersonId", "Stufe" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_profunda",
                table: "profunda",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_personen",
                table: "personen",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_otiumdefinitionperson",
                table: "otiumdefinitionperson",
                columns: new[] { "VerantwortlicheId", "VerwalteteOtiaId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_otiawiederholungen",
                table: "otiawiederholungen",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_otiatermine",
                table: "otiatermine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_otiakategorien",
                table: "otiakategorien",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_otiaeinschreibungsnotizen",
                table: "otiaeinschreibungsnotizen",
                columns: new[] { "BlockId", "StudentId", "AuthorId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_otiaeinschreibungen",
                table: "otiaeinschreibungen",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_otiaanwesenheiten",
                table: "otiaanwesenheiten",
                columns: new[] { "BlockId", "StudentId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_otia",
                table: "otia",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_mentormenteerelations",
                table: "mentormenteerelations",
                columns: new[] { "MentorId", "StudentId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_dataprotectionkeys",
                table: "dataprotectionkeys",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_calendarsubscriptions",
                table: "calendarsubscriptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_blocks",
                table: "blocks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_blocks_schultage_SchultagKey",
                table: "blocks",
                column: "SchultagKey",
                principalTable: "schultage",
                principalColumn: "Datum",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_calendarsubscriptions_personen_BetroffenePersonId",
                table: "calendarsubscriptions",
                column: "BetroffenePersonId",
                principalTable: "personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mentormenteerelations_personen_MentorId",
                table: "mentormenteerelations",
                column: "MentorId",
                principalTable: "personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mentormenteerelations_personen_StudentId",
                table: "mentormenteerelations",
                column: "StudentId",
                principalTable: "personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_otia_otiakategorien_KategorieId",
                table: "otia",
                column: "KategorieId",
                principalTable: "otiakategorien",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_otiaanwesenheiten_blocks_BlockId",
                table: "otiaanwesenheiten",
                column: "BlockId",
                principalTable: "blocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_otiaanwesenheiten_personen_StudentId",
                table: "otiaanwesenheiten",
                column: "StudentId",
                principalTable: "personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_otiaeinschreibungen_otiatermine_TerminId",
                table: "otiaeinschreibungen",
                column: "TerminId",
                principalTable: "otiatermine",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_otiaeinschreibungen_personen_BetroffenePersonId",
                table: "otiaeinschreibungen",
                column: "BetroffenePersonId",
                principalTable: "personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_otiaeinschreibungsnotizen_blocks_BlockId",
                table: "otiaeinschreibungsnotizen",
                column: "BlockId",
                principalTable: "blocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_otiaeinschreibungsnotizen_personen_AuthorId",
                table: "otiaeinschreibungsnotizen",
                column: "AuthorId",
                principalTable: "personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_otiaeinschreibungsnotizen_personen_StudentId",
                table: "otiaeinschreibungsnotizen",
                column: "StudentId",
                principalTable: "personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_otiakategorien_otiakategorien_ParentId",
                table: "otiakategorien",
                column: "ParentId",
                principalTable: "otiakategorien",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_otiatermine_blocks_BlockId",
                table: "otiatermine",
                column: "BlockId",
                principalTable: "blocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_otiatermine_otia_OtiumId",
                table: "otiatermine",
                column: "OtiumId",
                principalTable: "otia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_otiatermine_otiawiederholungen_WiederholungId",
                table: "otiatermine",
                column: "WiederholungId",
                principalTable: "otiawiederholungen",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_otiatermine_personen_TutorId",
                table: "otiatermine",
                column: "TutorId",
                principalTable: "personen",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_otiawiederholungen_otia_OtiumId",
                table: "otiawiederholungen",
                column: "OtiumId",
                principalTable: "otia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_otiawiederholungen_personen_TutorId",
                table: "otiawiederholungen",
                column: "TutorId",
                principalTable: "personen",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_otiumdefinitionperson_otia_VerwalteteOtiaId",
                table: "otiumdefinitionperson",
                column: "VerwalteteOtiaId",
                principalTable: "otia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_otiumdefinitionperson_personen_VerantwortlicheId",
                table: "otiumdefinitionperson",
                column: "VerantwortlicheId",
                principalTable: "personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_profunda_profundakategorien_KategorieId",
                table: "profunda",
                column: "KategorieId",
                principalTable: "profundakategorien",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_profundabelegwuensche_personen_BetroffenePersonId",
                table: "profundabelegwuensche",
                column: "BetroffenePersonId",
                principalTable: "personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_profundabelegwuensche_profundainstanzen_ProfundumInstanzId",
                table: "profundabelegwuensche",
                column: "ProfundumInstanzId",
                principalTable: "profundainstanzen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_profundaeinschreibungen_personen_BetroffenePersonId",
                table: "profundaeinschreibungen",
                column: "BetroffenePersonId",
                principalTable: "personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_profundaeinschreibungen_profundainstanzen_ProfundumInstanzId",
                table: "profundaeinschreibungen",
                column: "ProfundumInstanzId",
                principalTable: "profundainstanzen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_profundainstanzen_profunda_ProfundumId",
                table: "profundainstanzen",
                column: "ProfundumId",
                principalTable: "profunda",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_profundaslots_profundumeinwahlzeitraeume_EinwahlZeitraumId",
                table: "profundaslots",
                column: "EinwahlZeitraumId",
                principalTable: "profundumeinwahlzeitraeume",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_profunduminstanzprofundumslot_profundainstanzen_ProfundumIn~",
                table: "profunduminstanzprofundumslot",
                column: "ProfundumInstanzId",
                principalTable: "profundainstanzen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_profunduminstanzprofundumslot_profundaslots_SlotsId",
                table: "profunduminstanzprofundumslot",
                column: "SlotsId",
                principalTable: "profundaslots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_scheduledemails_personen_RecipientId",
                table: "scheduledemails",
                column: "RecipientId",
                principalTable: "personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_blocks_schultage_SchultagKey",
                table: "blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_calendarsubscriptions_personen_BetroffenePersonId",
                table: "calendarsubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_mentormenteerelations_personen_MentorId",
                table: "mentormenteerelations");

            migrationBuilder.DropForeignKey(
                name: "FK_mentormenteerelations_personen_StudentId",
                table: "mentormenteerelations");

            migrationBuilder.DropForeignKey(
                name: "FK_otia_otiakategorien_KategorieId",
                table: "otia");

            migrationBuilder.DropForeignKey(
                name: "FK_otiaanwesenheiten_blocks_BlockId",
                table: "otiaanwesenheiten");

            migrationBuilder.DropForeignKey(
                name: "FK_otiaanwesenheiten_personen_StudentId",
                table: "otiaanwesenheiten");

            migrationBuilder.DropForeignKey(
                name: "FK_otiaeinschreibungen_otiatermine_TerminId",
                table: "otiaeinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "FK_otiaeinschreibungen_personen_BetroffenePersonId",
                table: "otiaeinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "FK_otiaeinschreibungsnotizen_blocks_BlockId",
                table: "otiaeinschreibungsnotizen");

            migrationBuilder.DropForeignKey(
                name: "FK_otiaeinschreibungsnotizen_personen_AuthorId",
                table: "otiaeinschreibungsnotizen");

            migrationBuilder.DropForeignKey(
                name: "FK_otiaeinschreibungsnotizen_personen_StudentId",
                table: "otiaeinschreibungsnotizen");

            migrationBuilder.DropForeignKey(
                name: "FK_otiakategorien_otiakategorien_ParentId",
                table: "otiakategorien");

            migrationBuilder.DropForeignKey(
                name: "FK_otiatermine_blocks_BlockId",
                table: "otiatermine");

            migrationBuilder.DropForeignKey(
                name: "FK_otiatermine_otia_OtiumId",
                table: "otiatermine");

            migrationBuilder.DropForeignKey(
                name: "FK_otiatermine_otiawiederholungen_WiederholungId",
                table: "otiatermine");

            migrationBuilder.DropForeignKey(
                name: "FK_otiatermine_personen_TutorId",
                table: "otiatermine");

            migrationBuilder.DropForeignKey(
                name: "FK_otiawiederholungen_otia_OtiumId",
                table: "otiawiederholungen");

            migrationBuilder.DropForeignKey(
                name: "FK_otiawiederholungen_personen_TutorId",
                table: "otiawiederholungen");

            migrationBuilder.DropForeignKey(
                name: "FK_otiumdefinitionperson_otia_VerwalteteOtiaId",
                table: "otiumdefinitionperson");

            migrationBuilder.DropForeignKey(
                name: "FK_otiumdefinitionperson_personen_VerantwortlicheId",
                table: "otiumdefinitionperson");

            migrationBuilder.DropForeignKey(
                name: "FK_profunda_profundakategorien_KategorieId",
                table: "profunda");

            migrationBuilder.DropForeignKey(
                name: "FK_profundabelegwuensche_personen_BetroffenePersonId",
                table: "profundabelegwuensche");

            migrationBuilder.DropForeignKey(
                name: "FK_profundabelegwuensche_profundainstanzen_ProfundumInstanzId",
                table: "profundabelegwuensche");

            migrationBuilder.DropForeignKey(
                name: "FK_profundaeinschreibungen_personen_BetroffenePersonId",
                table: "profundaeinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "FK_profundaeinschreibungen_profundainstanzen_ProfundumInstanzId",
                table: "profundaeinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "FK_profundainstanzen_profunda_ProfundumId",
                table: "profundainstanzen");

            migrationBuilder.DropForeignKey(
                name: "FK_profundaslots_profundumeinwahlzeitraeume_EinwahlZeitraumId",
                table: "profundaslots");

            migrationBuilder.DropForeignKey(
                name: "FK_profunduminstanzprofundumslot_profundainstanzen_ProfundumIn~",
                table: "profunduminstanzprofundumslot");

            migrationBuilder.DropForeignKey(
                name: "FK_profunduminstanzprofundumslot_profundaslots_SlotsId",
                table: "profunduminstanzprofundumslot");

            migrationBuilder.DropForeignKey(
                name: "FK_scheduledemails_personen_RecipientId",
                table: "scheduledemails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_schultage",
                table: "schultage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_scheduledemails",
                table: "scheduledemails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_profunduminstanzprofundumslot",
                table: "profunduminstanzprofundumslot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_profundumeinwahlzeitraeume",
                table: "profundumeinwahlzeitraeume");

            migrationBuilder.DropPrimaryKey(
                name: "PK_profundaslots",
                table: "profundaslots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_profundakategorien",
                table: "profundakategorien");

            migrationBuilder.DropPrimaryKey(
                name: "PK_profundainstanzen",
                table: "profundainstanzen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_profundaeinschreibungen",
                table: "profundaeinschreibungen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_profundabelegwuensche",
                table: "profundabelegwuensche");

            migrationBuilder.DropPrimaryKey(
                name: "PK_profunda",
                table: "profunda");

            migrationBuilder.DropPrimaryKey(
                name: "PK_personen",
                table: "personen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_otiumdefinitionperson",
                table: "otiumdefinitionperson");

            migrationBuilder.DropPrimaryKey(
                name: "PK_otiawiederholungen",
                table: "otiawiederholungen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_otiatermine",
                table: "otiatermine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_otiakategorien",
                table: "otiakategorien");

            migrationBuilder.DropPrimaryKey(
                name: "PK_otiaeinschreibungsnotizen",
                table: "otiaeinschreibungsnotizen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_otiaeinschreibungen",
                table: "otiaeinschreibungen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_otiaanwesenheiten",
                table: "otiaanwesenheiten");

            migrationBuilder.DropPrimaryKey(
                name: "PK_otia",
                table: "otia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_mentormenteerelations",
                table: "mentormenteerelations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dataprotectionkeys",
                table: "dataprotectionkeys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_calendarsubscriptions",
                table: "calendarsubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_blocks",
                table: "blocks");

            migrationBuilder.RenameTable(
                name: "schultage",
                newName: "Schultage");

            migrationBuilder.RenameTable(
                name: "scheduledemails",
                newName: "ScheduledEmails");

            migrationBuilder.RenameTable(
                name: "profunduminstanzprofundumslot",
                newName: "ProfundumInstanzProfundumSlot");

            migrationBuilder.RenameTable(
                name: "profundumeinwahlzeitraeume",
                newName: "ProfundumEinwahlZeitraeume");

            migrationBuilder.RenameTable(
                name: "profundaslots",
                newName: "ProfundaSlots");

            migrationBuilder.RenameTable(
                name: "profundakategorien",
                newName: "ProfundaKategorien");

            migrationBuilder.RenameTable(
                name: "profundainstanzen",
                newName: "ProfundaInstanzen");

            migrationBuilder.RenameTable(
                name: "profundaeinschreibungen",
                newName: "ProfundaEinschreibungen");

            migrationBuilder.RenameTable(
                name: "profundabelegwuensche",
                newName: "ProfundaBelegWuensche");

            migrationBuilder.RenameTable(
                name: "profunda",
                newName: "Profunda");

            migrationBuilder.RenameTable(
                name: "personen",
                newName: "Personen");

            migrationBuilder.RenameTable(
                name: "otiumdefinitionperson",
                newName: "OtiumDefinitionPerson");

            migrationBuilder.RenameTable(
                name: "otiawiederholungen",
                newName: "OtiaWiederholungen");

            migrationBuilder.RenameTable(
                name: "otiatermine",
                newName: "OtiaTermine");

            migrationBuilder.RenameTable(
                name: "otiakategorien",
                newName: "OtiaKategorien");

            migrationBuilder.RenameTable(
                name: "otiaeinschreibungsnotizen",
                newName: "OtiaEinschreibungsNotizen");

            migrationBuilder.RenameTable(
                name: "otiaeinschreibungen",
                newName: "OtiaEinschreibungen");

            migrationBuilder.RenameTable(
                name: "otiaanwesenheiten",
                newName: "OtiaAnwesenheiten");

            migrationBuilder.RenameTable(
                name: "otia",
                newName: "Otia");

            migrationBuilder.RenameTable(
                name: "mentormenteerelations",
                newName: "MentorMenteeRelations");

            migrationBuilder.RenameTable(
                name: "dataprotectionkeys",
                newName: "DataProtectionKeys");

            migrationBuilder.RenameTable(
                name: "calendarsubscriptions",
                newName: "CalendarSubscriptions");

            migrationBuilder.RenameTable(
                name: "blocks",
                newName: "Blocks");

            migrationBuilder.RenameIndex(
                name: "IX_scheduledemails_RecipientId",
                table: "ScheduledEmails",
                newName: "IX_ScheduledEmails_RecipientId");

            migrationBuilder.RenameIndex(
                name: "IX_profunduminstanzprofundumslot_SlotsId",
                table: "ProfundumInstanzProfundumSlot",
                newName: "IX_ProfundumInstanzProfundumSlot_SlotsId");

            migrationBuilder.RenameIndex(
                name: "IX_profundaslots_EinwahlZeitraumId",
                table: "ProfundaSlots",
                newName: "IX_ProfundaSlots_EinwahlZeitraumId");

            migrationBuilder.RenameIndex(
                name: "IX_profundainstanzen_ProfundumId",
                table: "ProfundaInstanzen",
                newName: "IX_ProfundaInstanzen_ProfundumId");

            migrationBuilder.RenameIndex(
                name: "IX_profundaeinschreibungen_ProfundumInstanzId",
                table: "ProfundaEinschreibungen",
                newName: "IX_ProfundaEinschreibungen_ProfundumInstanzId");

            migrationBuilder.RenameIndex(
                name: "IX_profundabelegwuensche_BetroffenePersonId",
                table: "ProfundaBelegWuensche",
                newName: "IX_ProfundaBelegWuensche_BetroffenePersonId");

            migrationBuilder.RenameIndex(
                name: "IX_profunda_KategorieId",
                table: "Profunda",
                newName: "IX_Profunda_KategorieId");

            migrationBuilder.RenameIndex(
                name: "IX_otiumdefinitionperson_VerwalteteOtiaId",
                table: "OtiumDefinitionPerson",
                newName: "IX_OtiumDefinitionPerson_VerwalteteOtiaId");

            migrationBuilder.RenameIndex(
                name: "IX_otiawiederholungen_TutorId",
                table: "OtiaWiederholungen",
                newName: "IX_OtiaWiederholungen_TutorId");

            migrationBuilder.RenameIndex(
                name: "IX_otiawiederholungen_OtiumId",
                table: "OtiaWiederholungen",
                newName: "IX_OtiaWiederholungen_OtiumId");

            migrationBuilder.RenameIndex(
                name: "IX_otiatermine_WiederholungId",
                table: "OtiaTermine",
                newName: "IX_OtiaTermine_WiederholungId");

            migrationBuilder.RenameIndex(
                name: "IX_otiatermine_TutorId",
                table: "OtiaTermine",
                newName: "IX_OtiaTermine_TutorId");

            migrationBuilder.RenameIndex(
                name: "IX_otiatermine_OtiumId",
                table: "OtiaTermine",
                newName: "IX_OtiaTermine_OtiumId");

            migrationBuilder.RenameIndex(
                name: "IX_otiatermine_BlockId",
                table: "OtiaTermine",
                newName: "IX_OtiaTermine_BlockId");

            migrationBuilder.RenameIndex(
                name: "IX_otiakategorien_ParentId",
                table: "OtiaKategorien",
                newName: "IX_OtiaKategorien_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_otiaeinschreibungsnotizen_StudentId",
                table: "OtiaEinschreibungsNotizen",
                newName: "IX_OtiaEinschreibungsNotizen_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_otiaeinschreibungsnotizen_AuthorId",
                table: "OtiaEinschreibungsNotizen",
                newName: "IX_OtiaEinschreibungsNotizen_AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_otiaeinschreibungen_TerminId",
                table: "OtiaEinschreibungen",
                newName: "IX_OtiaEinschreibungen_TerminId");

            migrationBuilder.RenameIndex(
                name: "IX_otiaeinschreibungen_BetroffenePersonId",
                table: "OtiaEinschreibungen",
                newName: "IX_OtiaEinschreibungen_BetroffenePersonId");

            migrationBuilder.RenameIndex(
                name: "IX_otiaanwesenheiten_StudentId",
                table: "OtiaAnwesenheiten",
                newName: "IX_OtiaAnwesenheiten_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_otia_KategorieId",
                table: "Otia",
                newName: "IX_Otia_KategorieId");

            migrationBuilder.RenameIndex(
                name: "IX_mentormenteerelations_StudentId",
                table: "MentorMenteeRelations",
                newName: "IX_MentorMenteeRelations_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_calendarsubscriptions_BetroffenePersonId",
                table: "CalendarSubscriptions",
                newName: "IX_CalendarSubscriptions_BetroffenePersonId");

            migrationBuilder.RenameIndex(
                name: "IX_blocks_SchultagKey_SchemaId",
                table: "Blocks",
                newName: "IX_Blocks_SchultagKey_SchemaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Schultage",
                table: "Schultage",
                column: "Datum");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduledEmails",
                table: "ScheduledEmails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundumInstanzProfundumSlot",
                table: "ProfundumInstanzProfundumSlot",
                columns: new[] { "ProfundumInstanzId", "SlotsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundumEinwahlZeitraeume",
                table: "ProfundumEinwahlZeitraeume",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundaSlots",
                table: "ProfundaSlots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundaKategorien",
                table: "ProfundaKategorien",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundaInstanzen",
                table: "ProfundaInstanzen",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundaEinschreibungen",
                table: "ProfundaEinschreibungen",
                columns: new[] { "BetroffenePersonId", "ProfundumInstanzId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundaBelegWuensche",
                table: "ProfundaBelegWuensche",
                columns: new[] { "ProfundumInstanzId", "BetroffenePersonId", "Stufe" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Profunda",
                table: "Profunda",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Personen",
                table: "Personen",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtiumDefinitionPerson",
                table: "OtiumDefinitionPerson",
                columns: new[] { "VerantwortlicheId", "VerwalteteOtiaId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtiaWiederholungen",
                table: "OtiaWiederholungen",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtiaTermine",
                table: "OtiaTermine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtiaKategorien",
                table: "OtiaKategorien",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtiaEinschreibungsNotizen",
                table: "OtiaEinschreibungsNotizen",
                columns: new[] { "BlockId", "StudentId", "AuthorId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtiaEinschreibungen",
                table: "OtiaEinschreibungen",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtiaAnwesenheiten",
                table: "OtiaAnwesenheiten",
                columns: new[] { "BlockId", "StudentId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Otia",
                table: "Otia",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MentorMenteeRelations",
                table: "MentorMenteeRelations",
                columns: new[] { "MentorId", "StudentId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataProtectionKeys",
                table: "DataProtectionKeys",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CalendarSubscriptions",
                table: "CalendarSubscriptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Blocks",
                table: "Blocks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_Schultage_SchultagKey",
                table: "Blocks",
                column: "SchultagKey",
                principalTable: "Schultage",
                principalColumn: "Datum",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarSubscriptions_Personen_BetroffenePersonId",
                table: "CalendarSubscriptions",
                column: "BetroffenePersonId",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MentorMenteeRelations_Personen_MentorId",
                table: "MentorMenteeRelations",
                column: "MentorId",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MentorMenteeRelations_Personen_StudentId",
                table: "MentorMenteeRelations",
                column: "StudentId",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Otia_OtiaKategorien_KategorieId",
                table: "Otia",
                column: "KategorieId",
                principalTable: "OtiaKategorien",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaAnwesenheiten_Blocks_BlockId",
                table: "OtiaAnwesenheiten",
                column: "BlockId",
                principalTable: "Blocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaAnwesenheiten_Personen_StudentId",
                table: "OtiaAnwesenheiten",
                column: "StudentId",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaEinschreibungen_OtiaTermine_TerminId",
                table: "OtiaEinschreibungen",
                column: "TerminId",
                principalTable: "OtiaTermine",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaEinschreibungen_Personen_BetroffenePersonId",
                table: "OtiaEinschreibungen",
                column: "BetroffenePersonId",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaEinschreibungsNotizen_Blocks_BlockId",
                table: "OtiaEinschreibungsNotizen",
                column: "BlockId",
                principalTable: "Blocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaEinschreibungsNotizen_Personen_AuthorId",
                table: "OtiaEinschreibungsNotizen",
                column: "AuthorId",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaEinschreibungsNotizen_Personen_StudentId",
                table: "OtiaEinschreibungsNotizen",
                column: "StudentId",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaKategorien_OtiaKategorien_ParentId",
                table: "OtiaKategorien",
                column: "ParentId",
                principalTable: "OtiaKategorien",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaTermine_Blocks_BlockId",
                table: "OtiaTermine",
                column: "BlockId",
                principalTable: "Blocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaTermine_OtiaWiederholungen_WiederholungId",
                table: "OtiaTermine",
                column: "WiederholungId",
                principalTable: "OtiaWiederholungen",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaTermine_Otia_OtiumId",
                table: "OtiaTermine",
                column: "OtiumId",
                principalTable: "Otia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaTermine_Personen_TutorId",
                table: "OtiaTermine",
                column: "TutorId",
                principalTable: "Personen",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaWiederholungen_Otia_OtiumId",
                table: "OtiaWiederholungen",
                column: "OtiumId",
                principalTable: "Otia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtiaWiederholungen_Personen_TutorId",
                table: "OtiaWiederholungen",
                column: "TutorId",
                principalTable: "Personen",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OtiumDefinitionPerson_Otia_VerwalteteOtiaId",
                table: "OtiumDefinitionPerson",
                column: "VerwalteteOtiaId",
                principalTable: "Otia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtiumDefinitionPerson_Personen_VerantwortlicheId",
                table: "OtiumDefinitionPerson",
                column: "VerantwortlicheId",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Profunda_ProfundaKategorien_KategorieId",
                table: "Profunda",
                column: "KategorieId",
                principalTable: "ProfundaKategorien",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaInstanzen_Profunda_ProfundumId",
                table: "ProfundaInstanzen",
                column: "ProfundumId",
                principalTable: "Profunda",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaSlots_ProfundumEinwahlZeitraeume_EinwahlZeitraumId",
                table: "ProfundaSlots",
                column: "EinwahlZeitraumId",
                principalTable: "ProfundumEinwahlZeitraeume",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundumInstanzProfundumSlot_ProfundaInstanzen_ProfundumIn~",
                table: "ProfundumInstanzProfundumSlot",
                column: "ProfundumInstanzId",
                principalTable: "ProfundaInstanzen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundumInstanzProfundumSlot_ProfundaSlots_SlotsId",
                table: "ProfundumInstanzProfundumSlot",
                column: "SlotsId",
                principalTable: "ProfundaSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledEmails_Personen_RecipientId",
                table: "ScheduledEmails",
                column: "RecipientId",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
