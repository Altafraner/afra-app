using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AllColumnsLowerCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "Wochentyp",
                table: "schultage",
                newName: "wochentyp");

            migrationBuilder.RenameColumn(
                name: "Datum",
                table: "schultage",
                newName: "datum");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "scheduledemails",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "RecipientId",
                table: "scheduledemails",
                newName: "recipientid");

            migrationBuilder.RenameColumn(
                name: "Deadline",
                table: "scheduledemails",
                newName: "deadline");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "scheduledemails",
                newName: "body");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "scheduledemails",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_scheduledemails_RecipientId",
                table: "scheduledemails",
                newName: "ix_scheduledemails_recipientid");

            migrationBuilder.RenameColumn(
                name: "SlotsId",
                table: "profunduminstanzprofundumslot",
                newName: "slotsid");

            migrationBuilder.RenameColumn(
                name: "ProfundumInstanzId",
                table: "profunduminstanzprofundumslot",
                newName: "profunduminstanzid");

            migrationBuilder.RenameIndex(
                name: "IX_profunduminstanzprofundumslot_SlotsId",
                table: "profunduminstanzprofundumslot",
                newName: "ix_profunduminstanzprofundumslot_slotsid");

            migrationBuilder.RenameColumn(
                name: "HasBeenMatched",
                table: "profundumeinwahlzeitraeume",
                newName: "hasbeenmatched");

            migrationBuilder.RenameColumn(
                name: "EinwahlStop",
                table: "profundumeinwahlzeitraeume",
                newName: "einwahlstop");

            migrationBuilder.RenameColumn(
                name: "EinwahlStart",
                table: "profundumeinwahlzeitraeume",
                newName: "einwahlstart");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "profundumeinwahlzeitraeume",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Wochentag",
                table: "profundaslots",
                newName: "wochentag");

            migrationBuilder.RenameColumn(
                name: "Quartal",
                table: "profundaslots",
                newName: "quartal");

            migrationBuilder.RenameColumn(
                name: "Jahr",
                table: "profundaslots",
                newName: "jahr");

            migrationBuilder.RenameColumn(
                name: "EinwahlZeitraumId",
                table: "profundaslots",
                newName: "einwahlzeitraumid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "profundaslots",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_profundaslots_EinwahlZeitraumId",
                table: "profundaslots",
                newName: "ix_profundaslots_einwahlzeitraumid");

            migrationBuilder.RenameColumn(
                name: "ProfilProfundum",
                table: "profundakategorien",
                newName: "profilprofundum");

            migrationBuilder.RenameColumn(
                name: "MaxProEinwahl",
                table: "profundakategorien",
                newName: "maxproeinwahl");

            migrationBuilder.RenameColumn(
                name: "Bezeichnung",
                table: "profundakategorien",
                newName: "bezeichnung");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "profundakategorien",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProfundumId",
                table: "profundainstanzen",
                newName: "profundumid");

            migrationBuilder.RenameColumn(
                name: "MaxEinschreibungen",
                table: "profundainstanzen",
                newName: "maxeinschreibungen");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "profundainstanzen",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_profundainstanzen_ProfundumId",
                table: "profundainstanzen",
                newName: "ix_profundainstanzen_profundumid");

            migrationBuilder.RenameColumn(
                name: "ProfundumInstanzId",
                table: "profundaeinschreibungen",
                newName: "profunduminstanzid");

            migrationBuilder.RenameColumn(
                name: "BetroffenePersonId",
                table: "profundaeinschreibungen",
                newName: "betroffenepersonid");

            migrationBuilder.RenameIndex(
                name: "IX_profundaeinschreibungen_ProfundumInstanzId",
                table: "profundaeinschreibungen",
                newName: "ix_profundaeinschreibungen_profunduminstanzid");

            migrationBuilder.RenameColumn(
                name: "Stufe",
                table: "profundabelegwuensche",
                newName: "stufe");

            migrationBuilder.RenameColumn(
                name: "BetroffenePersonId",
                table: "profundabelegwuensche",
                newName: "betroffenepersonid");

            migrationBuilder.RenameColumn(
                name: "ProfundumInstanzId",
                table: "profundabelegwuensche",
                newName: "profunduminstanzid");

            migrationBuilder.RenameIndex(
                name: "IX_profundabelegwuensche_BetroffenePersonId",
                table: "profundabelegwuensche",
                newName: "ix_profundabelegwuensche_betroffenepersonid");

            migrationBuilder.RenameColumn(
                name: "MinKlasse",
                table: "profunda",
                newName: "minklasse");

            migrationBuilder.RenameColumn(
                name: "MaxKlasse",
                table: "profunda",
                newName: "maxklasse");

            migrationBuilder.RenameColumn(
                name: "KategorieId",
                table: "profunda",
                newName: "kategorieid");

            migrationBuilder.RenameColumn(
                name: "Bezeichnung",
                table: "profunda",
                newName: "bezeichnung");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "profunda",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_profunda_KategorieId",
                table: "profunda",
                newName: "ix_profunda_kategorieid");

            migrationBuilder.RenameColumn(
                name: "Rolle",
                table: "personen",
                newName: "rolle");

            migrationBuilder.RenameColumn(
                name: "LdapSyncTime",
                table: "personen",
                newName: "ldapsynctime");

            migrationBuilder.RenameColumn(
                name: "LdapSyncFailureTime",
                table: "personen",
                newName: "ldapsyncfailuretime");

            migrationBuilder.RenameColumn(
                name: "LdapObjectId",
                table: "personen",
                newName: "ldapobjectid");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "personen",
                newName: "lastname");

            migrationBuilder.RenameColumn(
                name: "Gruppe",
                table: "personen",
                newName: "gruppe");

            migrationBuilder.RenameColumn(
                name: "GlobalPermissions",
                table: "personen",
                newName: "globalpermissions");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "personen",
                newName: "firstname");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "personen",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "personen",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "VerwalteteOtiaId",
                table: "otiumdefinitionperson",
                newName: "verwalteteotiaid");

            migrationBuilder.RenameColumn(
                name: "VerantwortlicheId",
                table: "otiumdefinitionperson",
                newName: "verantwortlicheid");

            migrationBuilder.RenameIndex(
                name: "IX_otiumdefinitionperson_VerwalteteOtiaId",
                table: "otiumdefinitionperson",
                newName: "ix_otiumdefinitionperson_verwalteteotiaid");

            migrationBuilder.RenameColumn(
                name: "Wochentyp",
                table: "otiawiederholungen",
                newName: "wochentyp");

            migrationBuilder.RenameColumn(
                name: "Wochentag",
                table: "otiawiederholungen",
                newName: "wochentag");

            migrationBuilder.RenameColumn(
                name: "TutorId",
                table: "otiawiederholungen",
                newName: "tutorid");

            migrationBuilder.RenameColumn(
                name: "OtiumId",
                table: "otiawiederholungen",
                newName: "otiumid");

            migrationBuilder.RenameColumn(
                name: "Ort",
                table: "otiawiederholungen",
                newName: "ort");

            migrationBuilder.RenameColumn(
                name: "MaxEinschreibungen",
                table: "otiawiederholungen",
                newName: "maxeinschreibungen");

            migrationBuilder.RenameColumn(
                name: "Block",
                table: "otiawiederholungen",
                newName: "block");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "otiawiederholungen",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_otiawiederholungen_TutorId",
                table: "otiawiederholungen",
                newName: "ix_otiawiederholungen_tutorid");

            migrationBuilder.RenameIndex(
                name: "IX_otiawiederholungen_OtiumId",
                table: "otiawiederholungen",
                newName: "ix_otiawiederholungen_otiumid");

            migrationBuilder.RenameColumn(
                name: "WiederholungId",
                table: "otiatermine",
                newName: "wiederholungid");

            migrationBuilder.RenameColumn(
                name: "TutorId",
                table: "otiatermine",
                newName: "tutorid");

            migrationBuilder.RenameColumn(
                name: "SindAnwesenheitenKontrolliert",
                table: "otiatermine",
                newName: "sindanwesenheitenkontrolliert");

            migrationBuilder.RenameColumn(
                name: "OverrideBezeichnung",
                table: "otiatermine",
                newName: "overridebezeichnung");

            migrationBuilder.RenameColumn(
                name: "OverrideBeschreibung",
                table: "otiatermine",
                newName: "overridebeschreibung");

            migrationBuilder.RenameColumn(
                name: "OtiumId",
                table: "otiatermine",
                newName: "otiumid");

            migrationBuilder.RenameColumn(
                name: "Ort",
                table: "otiatermine",
                newName: "ort");

            migrationBuilder.RenameColumn(
                name: "MaxEinschreibungen",
                table: "otiatermine",
                newName: "maxeinschreibungen");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "otiatermine",
                newName: "lastmodified");

            migrationBuilder.RenameColumn(
                name: "IstAbgesagt",
                table: "otiatermine",
                newName: "istabgesagt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "otiatermine",
                newName: "createdat");

            migrationBuilder.RenameColumn(
                name: "BlockId",
                table: "otiatermine",
                newName: "blockid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "otiatermine",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_otiatermine_WiederholungId",
                table: "otiatermine",
                newName: "ix_otiatermine_wiederholungid");

            migrationBuilder.RenameIndex(
                name: "IX_otiatermine_TutorId",
                table: "otiatermine",
                newName: "ix_otiatermine_tutorid");

            migrationBuilder.RenameIndex(
                name: "IX_otiatermine_OtiumId",
                table: "otiatermine",
                newName: "ix_otiatermine_otiumid");

            migrationBuilder.RenameIndex(
                name: "IX_otiatermine_BlockId",
                table: "otiatermine",
                newName: "ix_otiatermine_blockid");

            migrationBuilder.RenameColumn(
                name: "Required",
                table: "otiakategorien",
                newName: "required");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "otiakategorien",
                newName: "parentid");

            migrationBuilder.RenameColumn(
                name: "IgnoreEnrollmentRule",
                table: "otiakategorien",
                newName: "ignoreenrollmentrule");

            migrationBuilder.RenameColumn(
                name: "Icon",
                table: "otiakategorien",
                newName: "icon");

            migrationBuilder.RenameColumn(
                name: "CssColor",
                table: "otiakategorien",
                newName: "csscolor");

            migrationBuilder.RenameColumn(
                name: "Bezeichnung",
                table: "otiakategorien",
                newName: "bezeichnung");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "otiakategorien",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_otiakategorien_ParentId",
                table: "otiakategorien",
                newName: "ix_otiakategorien_parentid");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "otiaeinschreibungsnotizen",
                newName: "lastmodified");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "otiaeinschreibungsnotizen",
                newName: "createdat");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "otiaeinschreibungsnotizen",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "otiaeinschreibungsnotizen",
                newName: "authorid");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "otiaeinschreibungsnotizen",
                newName: "studentid");

            migrationBuilder.RenameColumn(
                name: "BlockId",
                table: "otiaeinschreibungsnotizen",
                newName: "blockid");

            migrationBuilder.RenameIndex(
                name: "IX_otiaeinschreibungsnotizen_StudentId",
                table: "otiaeinschreibungsnotizen",
                newName: "ix_otiaeinschreibungsnotizen_studentid");

            migrationBuilder.RenameIndex(
                name: "IX_otiaeinschreibungsnotizen_AuthorId",
                table: "otiaeinschreibungsnotizen",
                newName: "ix_otiaeinschreibungsnotizen_authorid");

            migrationBuilder.RenameColumn(
                name: "TerminId",
                table: "otiaeinschreibungen",
                newName: "terminid");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "otiaeinschreibungen",
                newName: "lastmodified");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "otiaeinschreibungen",
                newName: "createdat");

            migrationBuilder.RenameColumn(
                name: "BetroffenePersonId",
                table: "otiaeinschreibungen",
                newName: "betroffenepersonid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "otiaeinschreibungen",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_otiaeinschreibungen_TerminId",
                table: "otiaeinschreibungen",
                newName: "ix_otiaeinschreibungen_terminid");

            migrationBuilder.RenameIndex(
                name: "IX_otiaeinschreibungen_BetroffenePersonId",
                table: "otiaeinschreibungen",
                newName: "ix_otiaeinschreibungen_betroffenepersonid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "otiaanwesenheiten",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "otiaanwesenheiten",
                newName: "studentid");

            migrationBuilder.RenameColumn(
                name: "BlockId",
                table: "otiaanwesenheiten",
                newName: "blockid");

            migrationBuilder.RenameIndex(
                name: "IX_otiaanwesenheiten_StudentId",
                table: "otiaanwesenheiten",
                newName: "ix_otiaanwesenheiten_studentid");

            migrationBuilder.RenameColumn(
                name: "MinKlasse",
                table: "otia",
                newName: "minklasse");

            migrationBuilder.RenameColumn(
                name: "MaxKlasse",
                table: "otia",
                newName: "maxklasse");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "otia",
                newName: "lastmodified");

            migrationBuilder.RenameColumn(
                name: "KategorieId",
                table: "otia",
                newName: "kategorieid");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "otia",
                newName: "createdat");

            migrationBuilder.RenameColumn(
                name: "Bezeichnung",
                table: "otia",
                newName: "bezeichnung");

            migrationBuilder.RenameColumn(
                name: "Beschreibung",
                table: "otia",
                newName: "beschreibung");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "otia",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_otia_KategorieId",
                table: "otia",
                newName: "ix_otia_kategorieid");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "mentormenteerelations",
                newName: "studentid");

            migrationBuilder.RenameColumn(
                name: "MentorId",
                table: "mentormenteerelations",
                newName: "mentorid");

            migrationBuilder.RenameIndex(
                name: "IX_mentormenteerelations_StudentId",
                table: "mentormenteerelations",
                newName: "ix_mentormenteerelations_studentid");

            migrationBuilder.RenameColumn(
                name: "Xml",
                table: "dataprotectionkeys",
                newName: "xml");

            migrationBuilder.RenameColumn(
                name: "FriendlyName",
                table: "dataprotectionkeys",
                newName: "friendlyname");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "dataprotectionkeys",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "BetroffenePersonId",
                table: "calendarsubscriptions",
                newName: "betroffenepersonid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "calendarsubscriptions",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_calendarsubscriptions_BetroffenePersonId",
                table: "calendarsubscriptions",
                newName: "ix_calendarsubscriptions_betroffenepersonid");

            migrationBuilder.RenameColumn(
                name: "SindAnwesenheitenFehlernderKontrolliert",
                table: "blocks",
                newName: "sindanwesenheitenfehlernderkontrolliert");

            migrationBuilder.RenameColumn(
                name: "SchultagKey",
                table: "blocks",
                newName: "schultagkey");

            migrationBuilder.RenameColumn(
                name: "SchemaId",
                table: "blocks",
                newName: "schemaid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "blocks",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_blocks_SchultagKey_SchemaId",
                table: "blocks",
                newName: "ix_blocks_schultagkey_schemaid");

            migrationBuilder.AddPrimaryKey(
                name: "pk_schultage",
                table: "schultage",
                column: "datum");

            migrationBuilder.AddPrimaryKey(
                name: "pk_scheduledemails",
                table: "scheduledemails",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunduminstanzprofundumslot",
                table: "profunduminstanzprofundumslot",
                columns: new[] { "profunduminstanzid", "slotsid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundumeinwahlzeitraeume",
                table: "profundumeinwahlzeitraeume",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundaslots",
                table: "profundaslots",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundakategorien",
                table: "profundakategorien",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundainstanzen",
                table: "profundainstanzen",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundaeinschreibungen",
                table: "profundaeinschreibungen",
                columns: new[] { "betroffenepersonid", "profunduminstanzid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundabelegwuensche",
                table: "profundabelegwuensche",
                columns: new[] { "profunduminstanzid", "betroffenepersonid", "stufe" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda",
                table: "profunda",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_personen",
                table: "personen",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_otiumdefinitionperson",
                table: "otiumdefinitionperson",
                columns: new[] { "verantwortlicheid", "verwalteteotiaid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_otiawiederholungen",
                table: "otiawiederholungen",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_otiatermine",
                table: "otiatermine",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_otiakategorien",
                table: "otiakategorien",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_otiaeinschreibungsnotizen",
                table: "otiaeinschreibungsnotizen",
                columns: new[] { "blockid", "studentid", "authorid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_otiaeinschreibungen",
                table: "otiaeinschreibungen",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_otiaanwesenheiten",
                table: "otiaanwesenheiten",
                columns: new[] { "blockid", "studentid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_otia",
                table: "otia",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_mentormenteerelations",
                table: "mentormenteerelations",
                columns: new[] { "mentorid", "studentid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_dataprotectionkeys",
                table: "dataprotectionkeys",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_calendarsubscriptions",
                table: "calendarsubscriptions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_blocks",
                table: "blocks",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_blocks_schultage_schultagkey",
                table: "blocks",
                column: "schultagkey",
                principalTable: "schultage",
                principalColumn: "datum",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_calendarsubscriptions_personen_betroffenepersonid",
                table: "calendarsubscriptions",
                column: "betroffenepersonid",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_mentormenteerelations_personen_mentorid",
                table: "mentormenteerelations",
                column: "mentorid",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_mentormenteerelations_personen_studentid",
                table: "mentormenteerelations",
                column: "studentid",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otia_otiakategorien_kategorieid",
                table: "otia",
                column: "kategorieid",
                principalTable: "otiakategorien",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otiaanwesenheiten_blocks_blockid",
                table: "otiaanwesenheiten",
                column: "blockid",
                principalTable: "blocks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otiaanwesenheiten_personen_studentid",
                table: "otiaanwesenheiten",
                column: "studentid",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otiaeinschreibungen_otiatermine_terminid",
                table: "otiaeinschreibungen",
                column: "terminid",
                principalTable: "otiatermine",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otiaeinschreibungen_personen_betroffenepersonid",
                table: "otiaeinschreibungen",
                column: "betroffenepersonid",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otiaeinschreibungsnotizen_blocks_blockid",
                table: "otiaeinschreibungsnotizen",
                column: "blockid",
                principalTable: "blocks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otiaeinschreibungsnotizen_personen_authorid",
                table: "otiaeinschreibungsnotizen",
                column: "authorid",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otiaeinschreibungsnotizen_personen_studentid",
                table: "otiaeinschreibungsnotizen",
                column: "studentid",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otiakategorien_otiakategorien_parentid",
                table: "otiakategorien",
                column: "parentid",
                principalTable: "otiakategorien",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_otiatermine_blocks_blockid",
                table: "otiatermine",
                column: "blockid",
                principalTable: "blocks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otiatermine_otia_otiumid",
                table: "otiatermine",
                column: "otiumid",
                principalTable: "otia",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otiatermine_otiawiederholungen_wiederholungid",
                table: "otiatermine",
                column: "wiederholungid",
                principalTable: "otiawiederholungen",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_otiatermine_personen_tutorid",
                table: "otiatermine",
                column: "tutorid",
                principalTable: "personen",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_otiawiederholungen_otia_otiumid",
                table: "otiawiederholungen",
                column: "otiumid",
                principalTable: "otia",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otiawiederholungen_personen_tutorid",
                table: "otiawiederholungen",
                column: "tutorid",
                principalTable: "personen",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_otiumdefinitionperson_otia_verwalteteotiaid",
                table: "otiumdefinitionperson",
                column: "verwalteteotiaid",
                principalTable: "otia",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otiumdefinitionperson_personen_verantwortlicheid",
                table: "otiumdefinitionperson",
                column: "verantwortlicheid",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profunda_profundakategorien_kategorieid",
                table: "profunda",
                column: "kategorieid",
                principalTable: "profundakategorien",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundabelegwuensche_personen_betroffenepersonid",
                table: "profundabelegwuensche",
                column: "betroffenepersonid",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundabelegwuensche_profundainstanzen_profunduminstanzid",
                table: "profundabelegwuensche",
                column: "profunduminstanzid",
                principalTable: "profundainstanzen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundaeinschreibungen_personen_betroffenepersonid",
                table: "profundaeinschreibungen",
                column: "betroffenepersonid",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundaeinschreibungen_profundainstanzen_profunduminstanzid",
                table: "profundaeinschreibungen",
                column: "profunduminstanzid",
                principalTable: "profundainstanzen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundainstanzen_profunda_profundumid",
                table: "profundainstanzen",
                column: "profundumid",
                principalTable: "profunda",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundaslots_profundumeinwahlzeitraeume_einwahlzeitraumid",
                table: "profundaslots",
                column: "einwahlzeitraumid",
                principalTable: "profundumeinwahlzeitraeume",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profunduminstanzprofundumslot_profundainstanzen_profundumin~",
                table: "profunduminstanzprofundumslot",
                column: "profunduminstanzid",
                principalTable: "profundainstanzen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profunduminstanzprofundumslot_profundaslots_slotsid",
                table: "profunduminstanzprofundumslot",
                column: "slotsid",
                principalTable: "profundaslots",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_scheduledemails_personen_recipientid",
                table: "scheduledemails",
                column: "recipientid",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_blocks_schultage_schultagkey",
                table: "blocks");

            migrationBuilder.DropForeignKey(
                name: "fk_calendarsubscriptions_personen_betroffenepersonid",
                table: "calendarsubscriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_mentormenteerelations_personen_mentorid",
                table: "mentormenteerelations");

            migrationBuilder.DropForeignKey(
                name: "fk_mentormenteerelations_personen_studentid",
                table: "mentormenteerelations");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_otiakategorien_kategorieid",
                table: "otia");

            migrationBuilder.DropForeignKey(
                name: "fk_otiaanwesenheiten_blocks_blockid",
                table: "otiaanwesenheiten");

            migrationBuilder.DropForeignKey(
                name: "fk_otiaanwesenheiten_personen_studentid",
                table: "otiaanwesenheiten");

            migrationBuilder.DropForeignKey(
                name: "fk_otiaeinschreibungen_otiatermine_terminid",
                table: "otiaeinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "fk_otiaeinschreibungen_personen_betroffenepersonid",
                table: "otiaeinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "fk_otiaeinschreibungsnotizen_blocks_blockid",
                table: "otiaeinschreibungsnotizen");

            migrationBuilder.DropForeignKey(
                name: "fk_otiaeinschreibungsnotizen_personen_authorid",
                table: "otiaeinschreibungsnotizen");

            migrationBuilder.DropForeignKey(
                name: "fk_otiaeinschreibungsnotizen_personen_studentid",
                table: "otiaeinschreibungsnotizen");

            migrationBuilder.DropForeignKey(
                name: "fk_otiakategorien_otiakategorien_parentid",
                table: "otiakategorien");

            migrationBuilder.DropForeignKey(
                name: "fk_otiatermine_blocks_blockid",
                table: "otiatermine");

            migrationBuilder.DropForeignKey(
                name: "fk_otiatermine_otia_otiumid",
                table: "otiatermine");

            migrationBuilder.DropForeignKey(
                name: "fk_otiatermine_otiawiederholungen_wiederholungid",
                table: "otiatermine");

            migrationBuilder.DropForeignKey(
                name: "fk_otiatermine_personen_tutorid",
                table: "otiatermine");

            migrationBuilder.DropForeignKey(
                name: "fk_otiawiederholungen_otia_otiumid",
                table: "otiawiederholungen");

            migrationBuilder.DropForeignKey(
                name: "fk_otiawiederholungen_personen_tutorid",
                table: "otiawiederholungen");

            migrationBuilder.DropForeignKey(
                name: "fk_otiumdefinitionperson_otia_verwalteteotiaid",
                table: "otiumdefinitionperson");

            migrationBuilder.DropForeignKey(
                name: "fk_otiumdefinitionperson_personen_verantwortlicheid",
                table: "otiumdefinitionperson");

            migrationBuilder.DropForeignKey(
                name: "fk_profunda_profundakategorien_kategorieid",
                table: "profunda");

            migrationBuilder.DropForeignKey(
                name: "fk_profundabelegwuensche_personen_betroffenepersonid",
                table: "profundabelegwuensche");

            migrationBuilder.DropForeignKey(
                name: "fk_profundabelegwuensche_profundainstanzen_profunduminstanzid",
                table: "profundabelegwuensche");

            migrationBuilder.DropForeignKey(
                name: "fk_profundaeinschreibungen_personen_betroffenepersonid",
                table: "profundaeinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "fk_profundaeinschreibungen_profundainstanzen_profunduminstanzid",
                table: "profundaeinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "fk_profundainstanzen_profunda_profundumid",
                table: "profundainstanzen");

            migrationBuilder.DropForeignKey(
                name: "fk_profundaslots_profundumeinwahlzeitraeume_einwahlzeitraumid",
                table: "profundaslots");

            migrationBuilder.DropForeignKey(
                name: "fk_profunduminstanzprofundumslot_profundainstanzen_profundumin~",
                table: "profunduminstanzprofundumslot");

            migrationBuilder.DropForeignKey(
                name: "fk_profunduminstanzprofundumslot_profundaslots_slotsid",
                table: "profunduminstanzprofundumslot");

            migrationBuilder.DropForeignKey(
                name: "fk_scheduledemails_personen_recipientid",
                table: "scheduledemails");

            migrationBuilder.DropPrimaryKey(
                name: "pk_schultage",
                table: "schultage");

            migrationBuilder.DropPrimaryKey(
                name: "pk_scheduledemails",
                table: "scheduledemails");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profunduminstanzprofundumslot",
                table: "profunduminstanzprofundumslot");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundumeinwahlzeitraeume",
                table: "profundumeinwahlzeitraeume");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundaslots",
                table: "profundaslots");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundakategorien",
                table: "profundakategorien");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundainstanzen",
                table: "profundainstanzen");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundaeinschreibungen",
                table: "profundaeinschreibungen");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundabelegwuensche",
                table: "profundabelegwuensche");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda",
                table: "profunda");

            migrationBuilder.DropPrimaryKey(
                name: "pk_personen",
                table: "personen");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otiumdefinitionperson",
                table: "otiumdefinitionperson");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otiawiederholungen",
                table: "otiawiederholungen");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otiatermine",
                table: "otiatermine");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otiakategorien",
                table: "otiakategorien");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otiaeinschreibungsnotizen",
                table: "otiaeinschreibungsnotizen");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otiaeinschreibungen",
                table: "otiaeinschreibungen");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otiaanwesenheiten",
                table: "otiaanwesenheiten");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otia",
                table: "otia");

            migrationBuilder.DropPrimaryKey(
                name: "pk_mentormenteerelations",
                table: "mentormenteerelations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_dataprotectionkeys",
                table: "dataprotectionkeys");

            migrationBuilder.DropPrimaryKey(
                name: "pk_calendarsubscriptions",
                table: "calendarsubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_blocks",
                table: "blocks");

            migrationBuilder.RenameColumn(
                name: "wochentyp",
                table: "schultage",
                newName: "Wochentyp");

            migrationBuilder.RenameColumn(
                name: "datum",
                table: "schultage",
                newName: "Datum");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "scheduledemails",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "recipientid",
                table: "scheduledemails",
                newName: "RecipientId");

            migrationBuilder.RenameColumn(
                name: "deadline",
                table: "scheduledemails",
                newName: "Deadline");

            migrationBuilder.RenameColumn(
                name: "body",
                table: "scheduledemails",
                newName: "Body");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "scheduledemails",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "ix_scheduledemails_recipientid",
                table: "scheduledemails",
                newName: "IX_scheduledemails_RecipientId");

            migrationBuilder.RenameColumn(
                name: "slotsid",
                table: "profunduminstanzprofundumslot",
                newName: "SlotsId");

            migrationBuilder.RenameColumn(
                name: "profunduminstanzid",
                table: "profunduminstanzprofundumslot",
                newName: "ProfundumInstanzId");

            migrationBuilder.RenameIndex(
                name: "ix_profunduminstanzprofundumslot_slotsid",
                table: "profunduminstanzprofundumslot",
                newName: "IX_profunduminstanzprofundumslot_SlotsId");

            migrationBuilder.RenameColumn(
                name: "hasbeenmatched",
                table: "profundumeinwahlzeitraeume",
                newName: "HasBeenMatched");

            migrationBuilder.RenameColumn(
                name: "einwahlstop",
                table: "profundumeinwahlzeitraeume",
                newName: "EinwahlStop");

            migrationBuilder.RenameColumn(
                name: "einwahlstart",
                table: "profundumeinwahlzeitraeume",
                newName: "EinwahlStart");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "profundumeinwahlzeitraeume",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "wochentag",
                table: "profundaslots",
                newName: "Wochentag");

            migrationBuilder.RenameColumn(
                name: "quartal",
                table: "profundaslots",
                newName: "Quartal");

            migrationBuilder.RenameColumn(
                name: "jahr",
                table: "profundaslots",
                newName: "Jahr");

            migrationBuilder.RenameColumn(
                name: "einwahlzeitraumid",
                table: "profundaslots",
                newName: "EinwahlZeitraumId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "profundaslots",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "ix_profundaslots_einwahlzeitraumid",
                table: "profundaslots",
                newName: "IX_profundaslots_EinwahlZeitraumId");

            migrationBuilder.RenameColumn(
                name: "profilprofundum",
                table: "profundakategorien",
                newName: "ProfilProfundum");

            migrationBuilder.RenameColumn(
                name: "maxproeinwahl",
                table: "profundakategorien",
                newName: "MaxProEinwahl");

            migrationBuilder.RenameColumn(
                name: "bezeichnung",
                table: "profundakategorien",
                newName: "Bezeichnung");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "profundakategorien",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "profundumid",
                table: "profundainstanzen",
                newName: "ProfundumId");

            migrationBuilder.RenameColumn(
                name: "maxeinschreibungen",
                table: "profundainstanzen",
                newName: "MaxEinschreibungen");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "profundainstanzen",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "ix_profundainstanzen_profundumid",
                table: "profundainstanzen",
                newName: "IX_profundainstanzen_ProfundumId");

            migrationBuilder.RenameColumn(
                name: "profunduminstanzid",
                table: "profundaeinschreibungen",
                newName: "ProfundumInstanzId");

            migrationBuilder.RenameColumn(
                name: "betroffenepersonid",
                table: "profundaeinschreibungen",
                newName: "BetroffenePersonId");

            migrationBuilder.RenameIndex(
                name: "ix_profundaeinschreibungen_profunduminstanzid",
                table: "profundaeinschreibungen",
                newName: "IX_profundaeinschreibungen_ProfundumInstanzId");

            migrationBuilder.RenameColumn(
                name: "stufe",
                table: "profundabelegwuensche",
                newName: "Stufe");

            migrationBuilder.RenameColumn(
                name: "betroffenepersonid",
                table: "profundabelegwuensche",
                newName: "BetroffenePersonId");

            migrationBuilder.RenameColumn(
                name: "profunduminstanzid",
                table: "profundabelegwuensche",
                newName: "ProfundumInstanzId");

            migrationBuilder.RenameIndex(
                name: "ix_profundabelegwuensche_betroffenepersonid",
                table: "profundabelegwuensche",
                newName: "IX_profundabelegwuensche_BetroffenePersonId");

            migrationBuilder.RenameColumn(
                name: "minklasse",
                table: "profunda",
                newName: "MinKlasse");

            migrationBuilder.RenameColumn(
                name: "maxklasse",
                table: "profunda",
                newName: "MaxKlasse");

            migrationBuilder.RenameColumn(
                name: "kategorieid",
                table: "profunda",
                newName: "KategorieId");

            migrationBuilder.RenameColumn(
                name: "bezeichnung",
                table: "profunda",
                newName: "Bezeichnung");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "profunda",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "ix_profunda_kategorieid",
                table: "profunda",
                newName: "IX_profunda_KategorieId");

            migrationBuilder.RenameColumn(
                name: "rolle",
                table: "personen",
                newName: "Rolle");

            migrationBuilder.RenameColumn(
                name: "ldapsynctime",
                table: "personen",
                newName: "LdapSyncTime");

            migrationBuilder.RenameColumn(
                name: "ldapsyncfailuretime",
                table: "personen",
                newName: "LdapSyncFailureTime");

            migrationBuilder.RenameColumn(
                name: "ldapobjectid",
                table: "personen",
                newName: "LdapObjectId");

            migrationBuilder.RenameColumn(
                name: "lastname",
                table: "personen",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "gruppe",
                table: "personen",
                newName: "Gruppe");

            migrationBuilder.RenameColumn(
                name: "globalpermissions",
                table: "personen",
                newName: "GlobalPermissions");

            migrationBuilder.RenameColumn(
                name: "firstname",
                table: "personen",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "personen",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "personen",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "verwalteteotiaid",
                table: "otiumdefinitionperson",
                newName: "VerwalteteOtiaId");

            migrationBuilder.RenameColumn(
                name: "verantwortlicheid",
                table: "otiumdefinitionperson",
                newName: "VerantwortlicheId");

            migrationBuilder.RenameIndex(
                name: "ix_otiumdefinitionperson_verwalteteotiaid",
                table: "otiumdefinitionperson",
                newName: "IX_otiumdefinitionperson_VerwalteteOtiaId");

            migrationBuilder.RenameColumn(
                name: "wochentyp",
                table: "otiawiederholungen",
                newName: "Wochentyp");

            migrationBuilder.RenameColumn(
                name: "wochentag",
                table: "otiawiederholungen",
                newName: "Wochentag");

            migrationBuilder.RenameColumn(
                name: "tutorid",
                table: "otiawiederholungen",
                newName: "TutorId");

            migrationBuilder.RenameColumn(
                name: "otiumid",
                table: "otiawiederholungen",
                newName: "OtiumId");

            migrationBuilder.RenameColumn(
                name: "ort",
                table: "otiawiederholungen",
                newName: "Ort");

            migrationBuilder.RenameColumn(
                name: "maxeinschreibungen",
                table: "otiawiederholungen",
                newName: "MaxEinschreibungen");

            migrationBuilder.RenameColumn(
                name: "block",
                table: "otiawiederholungen",
                newName: "Block");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "otiawiederholungen",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "ix_otiawiederholungen_tutorid",
                table: "otiawiederholungen",
                newName: "IX_otiawiederholungen_TutorId");

            migrationBuilder.RenameIndex(
                name: "ix_otiawiederholungen_otiumid",
                table: "otiawiederholungen",
                newName: "IX_otiawiederholungen_OtiumId");

            migrationBuilder.RenameColumn(
                name: "wiederholungid",
                table: "otiatermine",
                newName: "WiederholungId");

            migrationBuilder.RenameColumn(
                name: "tutorid",
                table: "otiatermine",
                newName: "TutorId");

            migrationBuilder.RenameColumn(
                name: "sindanwesenheitenkontrolliert",
                table: "otiatermine",
                newName: "SindAnwesenheitenKontrolliert");

            migrationBuilder.RenameColumn(
                name: "overridebezeichnung",
                table: "otiatermine",
                newName: "OverrideBezeichnung");

            migrationBuilder.RenameColumn(
                name: "overridebeschreibung",
                table: "otiatermine",
                newName: "OverrideBeschreibung");

            migrationBuilder.RenameColumn(
                name: "otiumid",
                table: "otiatermine",
                newName: "OtiumId");

            migrationBuilder.RenameColumn(
                name: "ort",
                table: "otiatermine",
                newName: "Ort");

            migrationBuilder.RenameColumn(
                name: "maxeinschreibungen",
                table: "otiatermine",
                newName: "MaxEinschreibungen");

            migrationBuilder.RenameColumn(
                name: "lastmodified",
                table: "otiatermine",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "istabgesagt",
                table: "otiatermine",
                newName: "IstAbgesagt");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "otiatermine",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "blockid",
                table: "otiatermine",
                newName: "BlockId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "otiatermine",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "ix_otiatermine_wiederholungid",
                table: "otiatermine",
                newName: "IX_otiatermine_WiederholungId");

            migrationBuilder.RenameIndex(
                name: "ix_otiatermine_tutorid",
                table: "otiatermine",
                newName: "IX_otiatermine_TutorId");

            migrationBuilder.RenameIndex(
                name: "ix_otiatermine_otiumid",
                table: "otiatermine",
                newName: "IX_otiatermine_OtiumId");

            migrationBuilder.RenameIndex(
                name: "ix_otiatermine_blockid",
                table: "otiatermine",
                newName: "IX_otiatermine_BlockId");

            migrationBuilder.RenameColumn(
                name: "required",
                table: "otiakategorien",
                newName: "Required");

            migrationBuilder.RenameColumn(
                name: "parentid",
                table: "otiakategorien",
                newName: "ParentId");

            migrationBuilder.RenameColumn(
                name: "ignoreenrollmentrule",
                table: "otiakategorien",
                newName: "IgnoreEnrollmentRule");

            migrationBuilder.RenameColumn(
                name: "icon",
                table: "otiakategorien",
                newName: "Icon");

            migrationBuilder.RenameColumn(
                name: "csscolor",
                table: "otiakategorien",
                newName: "CssColor");

            migrationBuilder.RenameColumn(
                name: "bezeichnung",
                table: "otiakategorien",
                newName: "Bezeichnung");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "otiakategorien",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "ix_otiakategorien_parentid",
                table: "otiakategorien",
                newName: "IX_otiakategorien_ParentId");

            migrationBuilder.RenameColumn(
                name: "lastmodified",
                table: "otiaeinschreibungsnotizen",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "otiaeinschreibungsnotizen",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "otiaeinschreibungsnotizen",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "authorid",
                table: "otiaeinschreibungsnotizen",
                newName: "AuthorId");

            migrationBuilder.RenameColumn(
                name: "studentid",
                table: "otiaeinschreibungsnotizen",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "blockid",
                table: "otiaeinschreibungsnotizen",
                newName: "BlockId");

            migrationBuilder.RenameIndex(
                name: "ix_otiaeinschreibungsnotizen_studentid",
                table: "otiaeinschreibungsnotizen",
                newName: "IX_otiaeinschreibungsnotizen_StudentId");

            migrationBuilder.RenameIndex(
                name: "ix_otiaeinschreibungsnotizen_authorid",
                table: "otiaeinschreibungsnotizen",
                newName: "IX_otiaeinschreibungsnotizen_AuthorId");

            migrationBuilder.RenameColumn(
                name: "terminid",
                table: "otiaeinschreibungen",
                newName: "TerminId");

            migrationBuilder.RenameColumn(
                name: "lastmodified",
                table: "otiaeinschreibungen",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "otiaeinschreibungen",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "betroffenepersonid",
                table: "otiaeinschreibungen",
                newName: "BetroffenePersonId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "otiaeinschreibungen",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "ix_otiaeinschreibungen_terminid",
                table: "otiaeinschreibungen",
                newName: "IX_otiaeinschreibungen_TerminId");

            migrationBuilder.RenameIndex(
                name: "ix_otiaeinschreibungen_betroffenepersonid",
                table: "otiaeinschreibungen",
                newName: "IX_otiaeinschreibungen_BetroffenePersonId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "otiaanwesenheiten",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "studentid",
                table: "otiaanwesenheiten",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "blockid",
                table: "otiaanwesenheiten",
                newName: "BlockId");

            migrationBuilder.RenameIndex(
                name: "ix_otiaanwesenheiten_studentid",
                table: "otiaanwesenheiten",
                newName: "IX_otiaanwesenheiten_StudentId");

            migrationBuilder.RenameColumn(
                name: "minklasse",
                table: "otia",
                newName: "MinKlasse");

            migrationBuilder.RenameColumn(
                name: "maxklasse",
                table: "otia",
                newName: "MaxKlasse");

            migrationBuilder.RenameColumn(
                name: "lastmodified",
                table: "otia",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "kategorieid",
                table: "otia",
                newName: "KategorieId");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "otia",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "bezeichnung",
                table: "otia",
                newName: "Bezeichnung");

            migrationBuilder.RenameColumn(
                name: "beschreibung",
                table: "otia",
                newName: "Beschreibung");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "otia",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "ix_otia_kategorieid",
                table: "otia",
                newName: "IX_otia_KategorieId");

            migrationBuilder.RenameColumn(
                name: "studentid",
                table: "mentormenteerelations",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "mentorid",
                table: "mentormenteerelations",
                newName: "MentorId");

            migrationBuilder.RenameIndex(
                name: "ix_mentormenteerelations_studentid",
                table: "mentormenteerelations",
                newName: "IX_mentormenteerelations_StudentId");

            migrationBuilder.RenameColumn(
                name: "xml",
                table: "dataprotectionkeys",
                newName: "Xml");

            migrationBuilder.RenameColumn(
                name: "friendlyname",
                table: "dataprotectionkeys",
                newName: "FriendlyName");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "dataprotectionkeys",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "betroffenepersonid",
                table: "calendarsubscriptions",
                newName: "BetroffenePersonId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "calendarsubscriptions",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "ix_calendarsubscriptions_betroffenepersonid",
                table: "calendarsubscriptions",
                newName: "IX_calendarsubscriptions_BetroffenePersonId");

            migrationBuilder.RenameColumn(
                name: "sindanwesenheitenfehlernderkontrolliert",
                table: "blocks",
                newName: "SindAnwesenheitenFehlernderKontrolliert");

            migrationBuilder.RenameColumn(
                name: "schultagkey",
                table: "blocks",
                newName: "SchultagKey");

            migrationBuilder.RenameColumn(
                name: "schemaid",
                table: "blocks",
                newName: "SchemaId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "blocks",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "ix_blocks_schultagkey_schemaid",
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
    }
}
