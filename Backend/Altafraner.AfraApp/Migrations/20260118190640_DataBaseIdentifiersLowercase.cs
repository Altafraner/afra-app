using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class DataBaseIdentifiersLowercase : Migration
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
                name: "FK_PersonProfundumInstanz_Personen_VerantwortlicheId",
                table: "PersonProfundumInstanz");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonProfundumInstanz_ProfundaInstanzen_BetreuteProfundaId",
                table: "PersonProfundumInstanz");

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
                name: "FK_ProfundaBelegWuensche_ProfundumEinwahlZeitraeume_EinwahlZei~",
                table: "ProfundaBelegWuensche");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaEinschreibungen_Personen_BetroffenePersonId",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaEinschreibungen_ProfundaInstanzen_ProfundumInstanzId",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaEinschreibungen_ProfundaSlots_SlotId",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaInstanzen_Profunda_ProfundumId",
                table: "ProfundaInstanzen");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaSlots_ProfundumEinwahlZeitraeume_EinwahlZeitraumId",
                table: "ProfundaSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundaTermine_ProfundaSlots_SlotId",
                table: "ProfundaTermine");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumDefinitionDependencies_Profunda_DependantId",
                table: "ProfundumDefinitionDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumDefinitionDependencies_Profunda_DependencyId",
                table: "ProfundumDefinitionDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumDefinitionProfundumFachbereich_ProfundaFachbereich~",
                table: "ProfundumDefinitionProfundumFachbereich");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumDefinitionProfundumFachbereich_Profunda_ProfundaId",
                table: "ProfundumDefinitionProfundumFachbereich");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumFachbereichProfundumFeedbackKategorie_ProfundaFach~",
                table: "ProfundumFachbereichProfundumFeedbackKategorie");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumFachbereichProfundumFeedbackKategorie_ProfundumFee~",
                table: "ProfundumFachbereichProfundumFeedbackKategorie");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumFeedbackAnker_ProfundumFeedbackKategories_Kategori~",
                table: "ProfundumFeedbackAnker");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumFeedbackEntries_Personen_BetroffenePersonId",
                table: "ProfundumFeedbackEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumFeedbackEntries_ProfundaInstanzen_InstanzId",
                table: "ProfundumFeedbackEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumFeedbackEntries_ProfundumFeedbackAnker_AnkerId",
                table: "ProfundumFeedbackEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumInstanzProfundumSlot_ProfundaInstanzen_ProfundumIn~",
                table: "ProfundumInstanzProfundumSlot");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumInstanzProfundumSlot_ProfundaSlots_SlotsId",
                table: "ProfundumInstanzProfundumSlot");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfundumProfilBefreiungen_Personen_BetroffenePersonId",
                table: "ProfundumProfilBefreiungen");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledEmails_Personen_RecipientId",
                table: "ScheduledEmails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Schultage",
                table: "Schultage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Profunda",
                table: "Profunda");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Personen",
                table: "Personen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Otia",
                table: "Otia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Blocks",
                table: "Blocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduledEmails",
                table: "ScheduledEmails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundumProfilBefreiungen",
                table: "ProfundumProfilBefreiungen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundumInstanzProfundumSlot",
                table: "ProfundumInstanzProfundumSlot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundumFeedbackKategories",
                table: "ProfundumFeedbackKategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundumFeedbackEntries",
                table: "ProfundumFeedbackEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundumFeedbackAnker",
                table: "ProfundumFeedbackAnker");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundumFachbereichProfundumFeedbackKategorie",
                table: "ProfundumFachbereichProfundumFeedbackKategorie");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundumEinwahlZeitraeume",
                table: "ProfundumEinwahlZeitraeume");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundumDefinitionProfundumFachbereich",
                table: "ProfundumDefinitionProfundumFachbereich");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundumDefinitionDependencies",
                table: "ProfundumDefinitionDependencies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundaTermine",
                table: "ProfundaTermine");

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
                name: "PK_ProfundaFachbereiche",
                table: "ProfundaFachbereiche");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundaEinschreibungen",
                table: "ProfundaEinschreibungen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfundaBelegWuensche",
                table: "ProfundaBelegWuensche");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonProfundumInstanz",
                table: "PersonProfundumInstanz");

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
                name: "PK_MentorMenteeRelations",
                table: "MentorMenteeRelations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataProtectionKeys",
                table: "DataProtectionKeys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CalendarSubscriptions",
                table: "CalendarSubscriptions");

            migrationBuilder.RenameTable(
                name: "Schultage",
                newName: "schultage");

            migrationBuilder.RenameTable(
                name: "Profunda",
                newName: "profunda");

            migrationBuilder.RenameTable(
                name: "Personen",
                newName: "personen");

            migrationBuilder.RenameTable(
                name: "Otia",
                newName: "otia");

            migrationBuilder.RenameTable(
                name: "Blocks",
                newName: "blocks");

            migrationBuilder.RenameTable(
                name: "ScheduledEmails",
                newName: "scheduled_emails");

            migrationBuilder.RenameTable(
                name: "ProfundumProfilBefreiungen",
                newName: "profundum_profil_befreiungen");

            migrationBuilder.RenameTable(
                name: "ProfundumInstanzProfundumSlot",
                newName: "profundum_instanz_profundum_slot");

            migrationBuilder.RenameTable(
                name: "ProfundumFeedbackKategories",
                newName: "profundum_feedback_kategories");

            migrationBuilder.RenameTable(
                name: "ProfundumFeedbackEntries",
                newName: "profundum_feedback_entries");

            migrationBuilder.RenameTable(
                name: "ProfundumFeedbackAnker",
                newName: "profundum_feedback_anker");

            migrationBuilder.RenameTable(
                name: "ProfundumFachbereichProfundumFeedbackKategorie",
                newName: "profundum_fachbereich_profundum_feedback_kategorie");

            migrationBuilder.RenameTable(
                name: "ProfundumEinwahlZeitraeume",
                newName: "profundum_einwahl_zeitraeume");

            migrationBuilder.RenameTable(
                name: "ProfundumDefinitionProfundumFachbereich",
                newName: "profundum_definition_profundum_fachbereich");

            migrationBuilder.RenameTable(
                name: "ProfundumDefinitionDependencies",
                newName: "profundum_definition_dependencies");

            migrationBuilder.RenameTable(
                name: "ProfundaTermine",
                newName: "profunda_termine");

            migrationBuilder.RenameTable(
                name: "ProfundaSlots",
                newName: "profunda_slots");

            migrationBuilder.RenameTable(
                name: "ProfundaKategorien",
                newName: "profunda_kategorien");

            migrationBuilder.RenameTable(
                name: "ProfundaInstanzen",
                newName: "profunda_instanzen");

            migrationBuilder.RenameTable(
                name: "ProfundaFachbereiche",
                newName: "profunda_fachbereiche");

            migrationBuilder.RenameTable(
                name: "ProfundaEinschreibungen",
                newName: "profunda_einschreibungen");

            migrationBuilder.RenameTable(
                name: "ProfundaBelegWuensche",
                newName: "profunda_beleg_wuensche");

            migrationBuilder.RenameTable(
                name: "PersonProfundumInstanz",
                newName: "person_profundum_instanz");

            migrationBuilder.RenameTable(
                name: "OtiumDefinitionPerson",
                newName: "otium_definition_person");

            migrationBuilder.RenameTable(
                name: "OtiaWiederholungen",
                newName: "otia_wiederholungen");

            migrationBuilder.RenameTable(
                name: "OtiaTermine",
                newName: "otia_termine");

            migrationBuilder.RenameTable(
                name: "OtiaKategorien",
                newName: "otia_kategorien");

            migrationBuilder.RenameTable(
                name: "OtiaEinschreibungsNotizen",
                newName: "otia_einschreibungs_notizen");

            migrationBuilder.RenameTable(
                name: "OtiaEinschreibungen",
                newName: "otia_einschreibungen");

            migrationBuilder.RenameTable(
                name: "OtiaAnwesenheiten",
                newName: "otia_anwesenheiten");

            migrationBuilder.RenameTable(
                name: "MentorMenteeRelations",
                newName: "mentor_mentee_relations");

            migrationBuilder.RenameTable(
                name: "DataProtectionKeys",
                newName: "data_protection_keys");

            migrationBuilder.RenameTable(
                name: "CalendarSubscriptions",
                newName: "calendar_subscriptions");

            migrationBuilder.RenameColumn(
                name: "Wochentyp",
                table: "schultage",
                newName: "wochentyp");

            migrationBuilder.RenameColumn(
                name: "Datum",
                table: "schultage",
                newName: "datum");

            migrationBuilder.RenameColumn(
                name: "Bezeichnung",
                table: "profunda",
                newName: "bezeichnung");

            migrationBuilder.RenameColumn(
                name: "Beschreibung",
                table: "profunda",
                newName: "beschreibung");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "profunda",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "MinKlasse",
                table: "profunda",
                newName: "min_klasse");

            migrationBuilder.RenameColumn(
                name: "MaxKlasse",
                table: "profunda",
                newName: "max_klasse");

            migrationBuilder.RenameColumn(
                name: "LastModifiedById",
                table: "profunda",
                newName: "last_modified_by_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "profunda",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "KategorieId",
                table: "profunda",
                newName: "kategorie_id");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "profunda",
                newName: "created_by_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "profunda",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Profunda_KategorieId",
                table: "profunda",
                newName: "ix_profunda_kategorie_id");

            migrationBuilder.RenameColumn(
                name: "Rolle",
                table: "personen",
                newName: "rolle");

            migrationBuilder.RenameColumn(
                name: "Gruppe",
                table: "personen",
                newName: "gruppe");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "personen",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "personen",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LdapSyncTime",
                table: "personen",
                newName: "ldap_sync_time");

            migrationBuilder.RenameColumn(
                name: "LdapSyncFailureTime",
                table: "personen",
                newName: "ldap_sync_failure_time");

            migrationBuilder.RenameColumn(
                name: "LdapObjectId",
                table: "personen",
                newName: "ldap_object_id");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "personen",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "GlobalPermissions",
                table: "personen",
                newName: "global_permissions");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "personen",
                newName: "first_name");

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

            migrationBuilder.RenameColumn(
                name: "MinKlasse",
                table: "otia",
                newName: "min_klasse");

            migrationBuilder.RenameColumn(
                name: "MaxKlasse",
                table: "otia",
                newName: "max_klasse");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "otia",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "KategorieId",
                table: "otia",
                newName: "kategorie_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "otia",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Otia_KategorieId",
                table: "otia",
                newName: "ix_otia_kategorie_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "blocks",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SindAnwesenheitenFehlernderKontrolliert",
                table: "blocks",
                newName: "sind_anwesenheiten_fehlernder_kontrolliert");

            migrationBuilder.RenameColumn(
                name: "SchultagKey",
                table: "blocks",
                newName: "schultag_key");

            migrationBuilder.RenameColumn(
                name: "SchemaId",
                table: "blocks",
                newName: "schema_id");

            migrationBuilder.RenameIndex(
                name: "IX_Blocks_SchultagKey_SchemaId",
                table: "blocks",
                newName: "ix_blocks_schultag_key_schema_id");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "scheduled_emails",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "Deadline",
                table: "scheduled_emails",
                newName: "deadline");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "scheduled_emails",
                newName: "body");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "scheduled_emails",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RecipientId",
                table: "scheduled_emails",
                newName: "recipient_id");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduledEmails_RecipientId",
                table: "scheduled_emails",
                newName: "ix_scheduled_emails_recipient_id");

            migrationBuilder.RenameColumn(
                name: "Quartal",
                table: "profundum_profil_befreiungen",
                newName: "quartal");

            migrationBuilder.RenameColumn(
                name: "Jahr",
                table: "profundum_profil_befreiungen",
                newName: "jahr");

            migrationBuilder.RenameColumn(
                name: "BetroffenePersonId",
                table: "profundum_profil_befreiungen",
                newName: "betroffene_person_id");

            migrationBuilder.RenameColumn(
                name: "SlotsId",
                table: "profundum_instanz_profundum_slot",
                newName: "slots_id");

            migrationBuilder.RenameColumn(
                name: "ProfundumInstanzId",
                table: "profundum_instanz_profundum_slot",
                newName: "profundum_instanz_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundumInstanzProfundumSlot_SlotsId",
                table: "profundum_instanz_profundum_slot",
                newName: "ix_profundum_instanz_profundum_slot_slots_id");

            migrationBuilder.RenameColumn(
                name: "Label",
                table: "profundum_feedback_kategories",
                newName: "label");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "profundum_feedback_kategories",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Grad",
                table: "profundum_feedback_entries",
                newName: "grad");

            migrationBuilder.RenameColumn(
                name: "BetroffenePersonId",
                table: "profundum_feedback_entries",
                newName: "betroffene_person_id");

            migrationBuilder.RenameColumn(
                name: "InstanzId",
                table: "profundum_feedback_entries",
                newName: "instanz_id");

            migrationBuilder.RenameColumn(
                name: "AnkerId",
                table: "profundum_feedback_entries",
                newName: "anker_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundumFeedbackEntries_InstanzId",
                table: "profundum_feedback_entries",
                newName: "ix_profundum_feedback_entries_instanz_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundumFeedbackEntries_BetroffenePersonId",
                table: "profundum_feedback_entries",
                newName: "ix_profundum_feedback_entries_betroffene_person_id");

            migrationBuilder.RenameColumn(
                name: "Label",
                table: "profundum_feedback_anker",
                newName: "label");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "profundum_feedback_anker",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "KategorieId",
                table: "profundum_feedback_anker",
                newName: "kategorie_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundumFeedbackAnker_KategorieId",
                table: "profundum_feedback_anker",
                newName: "ix_profundum_feedback_anker_kategorie_id");

            migrationBuilder.RenameColumn(
                name: "ProfundumFeedbackKategorieId",
                table: "profundum_fachbereich_profundum_feedback_kategorie",
                newName: "profundum_feedback_kategorie_id");

            migrationBuilder.RenameColumn(
                name: "FachbereicheId",
                table: "profundum_fachbereich_profundum_feedback_kategorie",
                newName: "fachbereiche_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundumFachbereichProfundumFeedbackKategorie_ProfundumFee~",
                table: "profundum_fachbereich_profundum_feedback_kategorie",
                newName: "ix_profundum_fachbereich_profundum_feedback_kategorie_profundu~");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "profundum_einwahl_zeitraeume",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "EinwahlStop",
                table: "profundum_einwahl_zeitraeume",
                newName: "einwahl_stop");

            migrationBuilder.RenameColumn(
                name: "EinwahlStart",
                table: "profundum_einwahl_zeitraeume",
                newName: "einwahl_start");

            migrationBuilder.RenameColumn(
                name: "ProfundaId",
                table: "profundum_definition_profundum_fachbereich",
                newName: "profunda_id");

            migrationBuilder.RenameColumn(
                name: "FachbereicheId",
                table: "profundum_definition_profundum_fachbereich",
                newName: "fachbereiche_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundumDefinitionProfundumFachbereich_ProfundaId",
                table: "profundum_definition_profundum_fachbereich",
                newName: "ix_profundum_definition_profundum_fachbereich_profunda_id");

            migrationBuilder.RenameColumn(
                name: "DependantId",
                table: "profundum_definition_dependencies",
                newName: "dependant_id");

            migrationBuilder.RenameColumn(
                name: "DependencyId",
                table: "profundum_definition_dependencies",
                newName: "dependency_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundumDefinitionDependencies_DependantId",
                table: "profundum_definition_dependencies",
                newName: "ix_profundum_definition_dependencies_dependant_id");

            migrationBuilder.RenameColumn(
                name: "Day",
                table: "profunda_termine",
                newName: "day");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "profunda_termine",
                newName: "start_time");

            migrationBuilder.RenameColumn(
                name: "SlotId",
                table: "profunda_termine",
                newName: "slot_id");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "profunda_termine",
                newName: "end_time");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaTermine_SlotId",
                table: "profunda_termine",
                newName: "ix_profunda_termine_slot_id");

            migrationBuilder.RenameColumn(
                name: "Wochentag",
                table: "profunda_slots",
                newName: "wochentag");

            migrationBuilder.RenameColumn(
                name: "Quartal",
                table: "profunda_slots",
                newName: "quartal");

            migrationBuilder.RenameColumn(
                name: "Jahr",
                table: "profunda_slots",
                newName: "jahr");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "profunda_slots",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "EinwahlZeitraumId",
                table: "profunda_slots",
                newName: "einwahl_zeitraum_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaSlots_EinwahlZeitraumId",
                table: "profunda_slots",
                newName: "ix_profunda_slots_einwahl_zeitraum_id");

            migrationBuilder.RenameColumn(
                name: "Bezeichnung",
                table: "profunda_kategorien",
                newName: "bezeichnung");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "profunda_kategorien",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProfilProfundum",
                table: "profunda_kategorien",
                newName: "profil_profundum");

            migrationBuilder.RenameColumn(
                name: "Ort",
                table: "profunda_instanzen",
                newName: "ort");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "profunda_instanzen",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProfundumId",
                table: "profunda_instanzen",
                newName: "profundum_id");

            migrationBuilder.RenameColumn(
                name: "MaxEinschreibungen",
                table: "profunda_instanzen",
                newName: "max_einschreibungen");

            migrationBuilder.RenameColumn(
                name: "LastModifiedById",
                table: "profunda_instanzen",
                newName: "last_modified_by_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "profunda_instanzen",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "profunda_instanzen",
                newName: "created_by_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "profunda_instanzen",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaInstanzen_ProfundumId",
                table: "profunda_instanzen",
                newName: "ix_profunda_instanzen_profundum_id");

            migrationBuilder.RenameColumn(
                name: "Label",
                table: "profunda_fachbereiche",
                newName: "label");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "profunda_fachbereiche",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProfundumInstanzId",
                table: "profunda_einschreibungen",
                newName: "profundum_instanz_id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedById",
                table: "profunda_einschreibungen",
                newName: "last_modified_by_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "profunda_einschreibungen",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsFixed",
                table: "profunda_einschreibungen",
                newName: "is_fixed");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "profunda_einschreibungen",
                newName: "created_by_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "profunda_einschreibungen",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "SlotId",
                table: "profunda_einschreibungen",
                newName: "slot_id");

            migrationBuilder.RenameColumn(
                name: "BetroffenePersonId",
                table: "profunda_einschreibungen",
                newName: "betroffene_person_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaEinschreibungen_SlotId",
                table: "profunda_einschreibungen",
                newName: "ix_profunda_einschreibungen_slot_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaEinschreibungen_ProfundumInstanzId",
                table: "profunda_einschreibungen",
                newName: "ix_profunda_einschreibungen_profundum_instanz_id");

            migrationBuilder.RenameColumn(
                name: "Stufe",
                table: "profunda_beleg_wuensche",
                newName: "stufe");

            migrationBuilder.RenameColumn(
                name: "EinwahlZeitraumId",
                table: "profunda_beleg_wuensche",
                newName: "einwahl_zeitraum_id");

            migrationBuilder.RenameColumn(
                name: "BetroffenePersonId",
                table: "profunda_beleg_wuensche",
                newName: "betroffene_person_id");

            migrationBuilder.RenameColumn(
                name: "ProfundumInstanzId",
                table: "profunda_beleg_wuensche",
                newName: "profundum_instanz_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaBelegWuensche_EinwahlZeitraumId",
                table: "profunda_beleg_wuensche",
                newName: "ix_profunda_beleg_wuensche_einwahl_zeitraum_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProfundaBelegWuensche_BetroffenePersonId",
                table: "profunda_beleg_wuensche",
                newName: "ix_profunda_beleg_wuensche_betroffene_person_id");

            migrationBuilder.RenameColumn(
                name: "VerantwortlicheId",
                table: "person_profundum_instanz",
                newName: "verantwortliche_id");

            migrationBuilder.RenameColumn(
                name: "BetreuteProfundaId",
                table: "person_profundum_instanz",
                newName: "betreute_profunda_id");

            migrationBuilder.RenameIndex(
                name: "IX_PersonProfundumInstanz_VerantwortlicheId",
                table: "person_profundum_instanz",
                newName: "ix_person_profundum_instanz_verantwortliche_id");

            migrationBuilder.RenameColumn(
                name: "VerwalteteOtiaId",
                table: "otium_definition_person",
                newName: "verwaltete_otia_id");

            migrationBuilder.RenameColumn(
                name: "VerantwortlicheId",
                table: "otium_definition_person",
                newName: "verantwortliche_id");

            migrationBuilder.RenameIndex(
                name: "IX_OtiumDefinitionPerson_VerwalteteOtiaId",
                table: "otium_definition_person",
                newName: "ix_otium_definition_person_verwaltete_otia_id");

            migrationBuilder.RenameColumn(
                name: "Wochentyp",
                table: "otia_wiederholungen",
                newName: "wochentyp");

            migrationBuilder.RenameColumn(
                name: "Wochentag",
                table: "otia_wiederholungen",
                newName: "wochentag");

            migrationBuilder.RenameColumn(
                name: "Ort",
                table: "otia_wiederholungen",
                newName: "ort");

            migrationBuilder.RenameColumn(
                name: "Block",
                table: "otia_wiederholungen",
                newName: "block");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "otia_wiederholungen",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TutorId",
                table: "otia_wiederholungen",
                newName: "tutor_id");

            migrationBuilder.RenameColumn(
                name: "OtiumId",
                table: "otia_wiederholungen",
                newName: "otium_id");

            migrationBuilder.RenameColumn(
                name: "MaxEinschreibungen",
                table: "otia_wiederholungen",
                newName: "max_einschreibungen");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaWiederholungen_TutorId",
                table: "otia_wiederholungen",
                newName: "ix_otia_wiederholungen_tutor_id");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaWiederholungen_OtiumId",
                table: "otia_wiederholungen",
                newName: "ix_otia_wiederholungen_otium_id");

            migrationBuilder.RenameColumn(
                name: "Ort",
                table: "otia_termine",
                newName: "ort");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "otia_termine",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WiederholungId",
                table: "otia_termine",
                newName: "wiederholung_id");

            migrationBuilder.RenameColumn(
                name: "TutorId",
                table: "otia_termine",
                newName: "tutor_id");

            migrationBuilder.RenameColumn(
                name: "SindAnwesenheitenKontrolliert",
                table: "otia_termine",
                newName: "sind_anwesenheiten_kontrolliert");

            migrationBuilder.RenameColumn(
                name: "OverrideBezeichnung",
                table: "otia_termine",
                newName: "override_bezeichnung");

            migrationBuilder.RenameColumn(
                name: "OverrideBeschreibung",
                table: "otia_termine",
                newName: "override_beschreibung");

            migrationBuilder.RenameColumn(
                name: "OtiumId",
                table: "otia_termine",
                newName: "otium_id");

            migrationBuilder.RenameColumn(
                name: "MaxEinschreibungen",
                table: "otia_termine",
                newName: "max_einschreibungen");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "otia_termine",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IstAbgesagt",
                table: "otia_termine",
                newName: "ist_abgesagt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "otia_termine",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BlockId",
                table: "otia_termine",
                newName: "block_id");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaTermine_WiederholungId",
                table: "otia_termine",
                newName: "ix_otia_termine_wiederholung_id");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaTermine_TutorId",
                table: "otia_termine",
                newName: "ix_otia_termine_tutor_id");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaTermine_OtiumId",
                table: "otia_termine",
                newName: "ix_otia_termine_otium_id");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaTermine_BlockId",
                table: "otia_termine",
                newName: "ix_otia_termine_block_id");

            migrationBuilder.RenameColumn(
                name: "Icon",
                table: "otia_kategorien",
                newName: "icon");

            migrationBuilder.RenameColumn(
                name: "Bezeichnung",
                table: "otia_kategorien",
                newName: "bezeichnung");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "otia_kategorien",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RequiredIn",
                table: "otia_kategorien",
                newName: "required_in");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "otia_kategorien",
                newName: "parent_id");

            migrationBuilder.RenameColumn(
                name: "IgnoreEnrollmentRule",
                table: "otia_kategorien",
                newName: "ignore_enrollment_rule");

            migrationBuilder.RenameColumn(
                name: "CssColor",
                table: "otia_kategorien",
                newName: "css_color");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaKategorien_ParentId",
                table: "otia_kategorien",
                newName: "ix_otia_kategorien_parent_id");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "otia_einschreibungs_notizen",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "otia_einschreibungs_notizen",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "otia_einschreibungs_notizen",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "otia_einschreibungs_notizen",
                newName: "author_id");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "otia_einschreibungs_notizen",
                newName: "student_id");

            migrationBuilder.RenameColumn(
                name: "BlockId",
                table: "otia_einschreibungs_notizen",
                newName: "block_id");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaEinschreibungsNotizen_StudentId",
                table: "otia_einschreibungs_notizen",
                newName: "ix_otia_einschreibungs_notizen_student_id");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaEinschreibungsNotizen_AuthorId",
                table: "otia_einschreibungs_notizen",
                newName: "ix_otia_einschreibungs_notizen_author_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "otia_einschreibungen",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TerminId",
                table: "otia_einschreibungen",
                newName: "termin_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "otia_einschreibungen",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "otia_einschreibungen",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BetroffenePersonId",
                table: "otia_einschreibungen",
                newName: "betroffene_person_id");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaEinschreibungen_TerminId",
                table: "otia_einschreibungen",
                newName: "ix_otia_einschreibungen_termin_id");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaEinschreibungen_BetroffenePersonId",
                table: "otia_einschreibungen",
                newName: "ix_otia_einschreibungen_betroffene_person_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "otia_anwesenheiten",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "otia_anwesenheiten",
                newName: "student_id");

            migrationBuilder.RenameColumn(
                name: "BlockId",
                table: "otia_anwesenheiten",
                newName: "block_id");

            migrationBuilder.RenameIndex(
                name: "IX_OtiaAnwesenheiten_StudentId",
                table: "otia_anwesenheiten",
                newName: "ix_otia_anwesenheiten_student_id");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "mentor_mentee_relations",
                newName: "student_id");

            migrationBuilder.RenameColumn(
                name: "MentorId",
                table: "mentor_mentee_relations",
                newName: "mentor_id");

            migrationBuilder.RenameIndex(
                name: "IX_MentorMenteeRelations_StudentId",
                table: "mentor_mentee_relations",
                newName: "ix_mentor_mentee_relations_student_id");

            migrationBuilder.RenameColumn(
                name: "Xml",
                table: "data_protection_keys",
                newName: "xml");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "data_protection_keys",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "FriendlyName",
                table: "data_protection_keys",
                newName: "friendly_name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "calendar_subscriptions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "BetroffenePersonId",
                table: "calendar_subscriptions",
                newName: "betroffene_person_id");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarSubscriptions_BetroffenePersonId",
                table: "calendar_subscriptions",
                newName: "ix_calendar_subscriptions_betroffene_person_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_schultage",
                table: "schultage",
                column: "datum");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda",
                table: "profunda",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_personen",
                table: "personen",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_otia",
                table: "otia",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_blocks",
                table: "blocks",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_scheduled_emails",
                table: "scheduled_emails",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundum_profil_befreiungen",
                table: "profundum_profil_befreiungen",
                columns: new[] { "betroffene_person_id", "jahr", "quartal" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundum_instanz_profundum_slot",
                table: "profundum_instanz_profundum_slot",
                columns: new[] { "profundum_instanz_id", "slots_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundum_feedback_kategories",
                table: "profundum_feedback_kategories",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundum_feedback_entries",
                table: "profundum_feedback_entries",
                columns: new[] { "anker_id", "instanz_id", "betroffene_person_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundum_feedback_anker",
                table: "profundum_feedback_anker",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundum_fachbereich_profundum_feedback_kategorie",
                table: "profundum_fachbereich_profundum_feedback_kategorie",
                columns: new[] { "fachbereiche_id", "profundum_feedback_kategorie_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundum_einwahl_zeitraeume",
                table: "profundum_einwahl_zeitraeume",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundum_definition_profundum_fachbereich",
                table: "profundum_definition_profundum_fachbereich",
                columns: new[] { "fachbereiche_id", "profunda_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_profundum_definition_dependencies",
                table: "profundum_definition_dependencies",
                columns: new[] { "dependency_id", "dependant_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda_termine",
                table: "profunda_termine",
                column: "day");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda_slots",
                table: "profunda_slots",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda_kategorien",
                table: "profunda_kategorien",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda_instanzen",
                table: "profunda_instanzen",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda_fachbereiche",
                table: "profunda_fachbereiche",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda_einschreibungen",
                table: "profunda_einschreibungen",
                columns: new[] { "betroffene_person_id", "slot_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_profunda_beleg_wuensche",
                table: "profunda_beleg_wuensche",
                columns: new[] { "profundum_instanz_id", "betroffene_person_id", "stufe" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_person_profundum_instanz",
                table: "person_profundum_instanz",
                columns: new[] { "betreute_profunda_id", "verantwortliche_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_otium_definition_person",
                table: "otium_definition_person",
                columns: new[] { "verantwortliche_id", "verwaltete_otia_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_otia_wiederholungen",
                table: "otia_wiederholungen",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_otia_termine",
                table: "otia_termine",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_otia_kategorien",
                table: "otia_kategorien",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_otia_einschreibungs_notizen",
                table: "otia_einschreibungs_notizen",
                columns: new[] { "block_id", "student_id", "author_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_otia_einschreibungen",
                table: "otia_einschreibungen",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_otia_anwesenheiten",
                table: "otia_anwesenheiten",
                columns: new[] { "block_id", "student_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_mentor_mentee_relations",
                table: "mentor_mentee_relations",
                columns: new[] { "mentor_id", "student_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_data_protection_keys",
                table: "data_protection_keys",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_calendar_subscriptions",
                table: "calendar_subscriptions",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_blocks_schultage_schultag_key",
                table: "blocks",
                column: "schultag_key",
                principalTable: "schultage",
                principalColumn: "datum",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_calendar_subscriptions_personen_betroffene_person_id",
                table: "calendar_subscriptions",
                column: "betroffene_person_id",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_mentor_mentee_relations_personen_mentor_id",
                table: "mentor_mentee_relations",
                column: "mentor_id",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_mentor_mentee_relations_personen_student_id",
                table: "mentor_mentee_relations",
                column: "student_id",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otia_otia_kategorien_kategorie_id",
                table: "otia",
                column: "kategorie_id",
                principalTable: "otia_kategorien",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otia_anwesenheiten_blocks_block_id",
                table: "otia_anwesenheiten",
                column: "block_id",
                principalTable: "blocks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otia_anwesenheiten_personen_student_id",
                table: "otia_anwesenheiten",
                column: "student_id",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otia_einschreibungen_otia_termine_termin_id",
                table: "otia_einschreibungen",
                column: "termin_id",
                principalTable: "otia_termine",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otia_einschreibungen_personen_betroffene_person_id",
                table: "otia_einschreibungen",
                column: "betroffene_person_id",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otia_einschreibungs_notizen_blocks_block_id",
                table: "otia_einschreibungs_notizen",
                column: "block_id",
                principalTable: "blocks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otia_einschreibungs_notizen_personen_author_id",
                table: "otia_einschreibungs_notizen",
                column: "author_id",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otia_einschreibungs_notizen_personen_student_id",
                table: "otia_einschreibungs_notizen",
                column: "student_id",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otia_kategorien_otia_kategorien_parent_id",
                table: "otia_kategorien",
                column: "parent_id",
                principalTable: "otia_kategorien",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_otia_termine_blocks_block_id",
                table: "otia_termine",
                column: "block_id",
                principalTable: "blocks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otia_termine_otia_otium_id",
                table: "otia_termine",
                column: "otium_id",
                principalTable: "otia",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otia_termine_otia_wiederholungen_wiederholung_id",
                table: "otia_termine",
                column: "wiederholung_id",
                principalTable: "otia_wiederholungen",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_otia_termine_personen_tutor_id",
                table: "otia_termine",
                column: "tutor_id",
                principalTable: "personen",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_otia_wiederholungen_otia_otium_id",
                table: "otia_wiederholungen",
                column: "otium_id",
                principalTable: "otia",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otia_wiederholungen_personen_tutor_id",
                table: "otia_wiederholungen",
                column: "tutor_id",
                principalTable: "personen",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_otium_definition_person_otia_verwaltete_otia_id",
                table: "otium_definition_person",
                column: "verwaltete_otia_id",
                principalTable: "otia",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otium_definition_person_personen_verantwortliche_id",
                table: "otium_definition_person",
                column: "verantwortliche_id",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_person_profundum_instanz_personen_verantwortliche_id",
                table: "person_profundum_instanz",
                column: "verantwortliche_id",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_person_profundum_instanz_profunda_instanzen_betreute_profun~",
                table: "person_profundum_instanz",
                column: "betreute_profunda_id",
                principalTable: "profunda_instanzen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profunda_profunda_kategorien_kategorie_id",
                table: "profunda",
                column: "kategorie_id",
                principalTable: "profunda_kategorien",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profunda_beleg_wuensche_personen_betroffene_person_id",
                table: "profunda_beleg_wuensche",
                column: "betroffene_person_id",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profunda_beleg_wuensche_profunda_instanzen_profundum_instanz~",
                table: "profunda_beleg_wuensche",
                column: "profundum_instanz_id",
                principalTable: "profunda_instanzen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profunda_beleg_wuensche_profundum_einwahl_zeitraeume_einwahl_~",
                table: "profunda_beleg_wuensche",
                column: "einwahl_zeitraum_id",
                principalTable: "profundum_einwahl_zeitraeume",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profunda_einschreibungen_personen_betroffene_person_id",
                table: "profunda_einschreibungen",
                column: "betroffene_person_id",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profunda_einschreibungen_profunda_instanzen_profundum_instan~",
                table: "profunda_einschreibungen",
                column: "profundum_instanz_id",
                principalTable: "profunda_instanzen",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_profunda_einschreibungen_profunda_slots_slot_id",
                table: "profunda_einschreibungen",
                column: "slot_id",
                principalTable: "profunda_slots",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profunda_instanzen_profunda_profundum_id",
                table: "profunda_instanzen",
                column: "profundum_id",
                principalTable: "profunda",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profunda_slots_profundum_einwahl_zeitraeume_einwahl_zeitrau~",
                table: "profunda_slots",
                column: "einwahl_zeitraum_id",
                principalTable: "profundum_einwahl_zeitraeume",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profunda_termine_profunda_slots_slot_id",
                table: "profunda_termine",
                column: "slot_id",
                principalTable: "profunda_slots",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_definition_dependencies_profunda_dependant_id",
                table: "profundum_definition_dependencies",
                column: "dependant_id",
                principalTable: "profunda",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_definition_dependencies_profunda_dependency_id",
                table: "profundum_definition_dependencies",
                column: "dependency_id",
                principalTable: "profunda",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_definition_profundum_fachbereich_profunda_fachber~",
                table: "profundum_definition_profundum_fachbereich",
                column: "fachbereiche_id",
                principalTable: "profunda_fachbereiche",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_definition_profundum_fachbereich_profunda_profund~",
                table: "profundum_definition_profundum_fachbereich",
                column: "profunda_id",
                principalTable: "profunda",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_fachbereich_profundum_feedback_kategorie_profunda~",
                table: "profundum_fachbereich_profundum_feedback_kategorie",
                column: "fachbereiche_id",
                principalTable: "profunda_fachbereiche",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_fachbereich_profundum_feedback_kategorie_profundu~",
                table: "profundum_fachbereich_profundum_feedback_kategorie",
                column: "profundum_feedback_kategorie_id",
                principalTable: "profundum_feedback_kategories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_feedback_anker_profundum_feedback_kategories_katego~",
                table: "profundum_feedback_anker",
                column: "kategorie_id",
                principalTable: "profundum_feedback_kategories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_feedback_entries_personen_betroffene_person_id",
                table: "profundum_feedback_entries",
                column: "betroffene_person_id",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_feedback_entries_profunda_instanzen_instanz_id",
                table: "profundum_feedback_entries",
                column: "instanz_id",
                principalTable: "profunda_instanzen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_feedback_entries_profundum_feedback_anker_anker_id",
                table: "profundum_feedback_entries",
                column: "anker_id",
                principalTable: "profundum_feedback_anker",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_instanz_profundum_slot_profunda_instanzen_profund~",
                table: "profundum_instanz_profundum_slot",
                column: "profundum_instanz_id",
                principalTable: "profunda_instanzen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_instanz_profundum_slot_profunda_slots_slots_id",
                table: "profundum_instanz_profundum_slot",
                column: "slots_id",
                principalTable: "profunda_slots",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_profundum_profil_befreiungen_personen_betroffene_person_id",
                table: "profundum_profil_befreiungen",
                column: "betroffene_person_id",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_scheduled_emails_personen_recipient_id",
                table: "scheduled_emails",
                column: "recipient_id",
                principalTable: "personen",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_blocks_schultage_schultag_key",
                table: "blocks");

            migrationBuilder.DropForeignKey(
                name: "fk_calendar_subscriptions_personen_betroffene_person_id",
                table: "calendar_subscriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_mentor_mentee_relations_personen_mentor_id",
                table: "mentor_mentee_relations");

            migrationBuilder.DropForeignKey(
                name: "fk_mentor_mentee_relations_personen_student_id",
                table: "mentor_mentee_relations");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_otia_kategorien_kategorie_id",
                table: "otia");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_anwesenheiten_blocks_block_id",
                table: "otia_anwesenheiten");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_anwesenheiten_personen_student_id",
                table: "otia_anwesenheiten");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_einschreibungen_otia_termine_termin_id",
                table: "otia_einschreibungen");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_einschreibungen_personen_betroffene_person_id",
                table: "otia_einschreibungen");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_einschreibungs_notizen_blocks_block_id",
                table: "otia_einschreibungs_notizen");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_einschreibungs_notizen_personen_author_id",
                table: "otia_einschreibungs_notizen");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_einschreibungs_notizen_personen_student_id",
                table: "otia_einschreibungs_notizen");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_kategorien_otia_kategorien_parent_id",
                table: "otia_kategorien");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_termine_blocks_block_id",
                table: "otia_termine");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_termine_otia_otium_id",
                table: "otia_termine");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_termine_otia_wiederholungen_wiederholung_id",
                table: "otia_termine");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_termine_personen_tutor_id",
                table: "otia_termine");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_wiederholungen_otia_otium_id",
                table: "otia_wiederholungen");

            migrationBuilder.DropForeignKey(
                name: "fk_otia_wiederholungen_personen_tutor_id",
                table: "otia_wiederholungen");

            migrationBuilder.DropForeignKey(
                name: "fk_otium_definition_person_otia_verwaltete_otia_id",
                table: "otium_definition_person");

            migrationBuilder.DropForeignKey(
                name: "fk_otium_definition_person_personen_verantwortliche_id",
                table: "otium_definition_person");

            migrationBuilder.DropForeignKey(
                name: "fk_person_profundum_instanz_personen_verantwortliche_id",
                table: "person_profundum_instanz");

            migrationBuilder.DropForeignKey(
                name: "fk_person_profundum_instanz_profunda_instanzen_betreute_profun~",
                table: "person_profundum_instanz");

            migrationBuilder.DropForeignKey(
                name: "fk_profunda_profunda_kategorien_kategorie_id",
                table: "profunda");

            migrationBuilder.DropForeignKey(
                name: "fk_profunda_beleg_wuensche_personen_betroffene_person_id",
                table: "profunda_beleg_wuensche");

            migrationBuilder.DropForeignKey(
                name: "fk_profunda_beleg_wuensche_profunda_instanzen_profundum_instanz~",
                table: "profunda_beleg_wuensche");

            migrationBuilder.DropForeignKey(
                name: "fk_profunda_beleg_wuensche_profundum_einwahl_zeitraeume_einwahl_~",
                table: "profunda_beleg_wuensche");

            migrationBuilder.DropForeignKey(
                name: "fk_profunda_einschreibungen_personen_betroffene_person_id",
                table: "profunda_einschreibungen");

            migrationBuilder.DropForeignKey(
                name: "fk_profunda_einschreibungen_profunda_instanzen_profundum_instan~",
                table: "profunda_einschreibungen");

            migrationBuilder.DropForeignKey(
                name: "fk_profunda_einschreibungen_profunda_slots_slot_id",
                table: "profunda_einschreibungen");

            migrationBuilder.DropForeignKey(
                name: "fk_profunda_instanzen_profunda_profundum_id",
                table: "profunda_instanzen");

            migrationBuilder.DropForeignKey(
                name: "fk_profunda_slots_profundum_einwahl_zeitraeume_einwahl_zeitrau~",
                table: "profunda_slots");

            migrationBuilder.DropForeignKey(
                name: "fk_profunda_termine_profunda_slots_slot_id",
                table: "profunda_termine");

            migrationBuilder.DropForeignKey(
                name: "fk_profundum_definition_dependencies_profunda_dependant_id",
                table: "profundum_definition_dependencies");

            migrationBuilder.DropForeignKey(
                name: "fk_profundum_definition_dependencies_profunda_dependency_id",
                table: "profundum_definition_dependencies");

            migrationBuilder.DropForeignKey(
                name: "fk_profundum_definition_profundum_fachbereich_profunda_fachber~",
                table: "profundum_definition_profundum_fachbereich");

            migrationBuilder.DropForeignKey(
                name: "fk_profundum_definition_profundum_fachbereich_profunda_profund~",
                table: "profundum_definition_profundum_fachbereich");

            migrationBuilder.DropForeignKey(
                name: "fk_profundum_fachbereich_profundum_feedback_kategorie_profunda~",
                table: "profundum_fachbereich_profundum_feedback_kategorie");

            migrationBuilder.DropForeignKey(
                name: "fk_profundum_fachbereich_profundum_feedback_kategorie_profundu~",
                table: "profundum_fachbereich_profundum_feedback_kategorie");

            migrationBuilder.DropForeignKey(
                name: "fk_profundum_feedback_anker_profundum_feedback_kategories_katego~",
                table: "profundum_feedback_anker");

            migrationBuilder.DropForeignKey(
                name: "fk_profundum_feedback_entries_personen_betroffene_person_id",
                table: "profundum_feedback_entries");

            migrationBuilder.DropForeignKey(
                name: "fk_profundum_feedback_entries_profunda_instanzen_instanz_id",
                table: "profundum_feedback_entries");

            migrationBuilder.DropForeignKey(
                name: "fk_profundum_feedback_entries_profundum_feedback_anker_anker_id",
                table: "profundum_feedback_entries");

            migrationBuilder.DropForeignKey(
                name: "fk_profundum_instanz_profundum_slot_profunda_instanzen_profund~",
                table: "profundum_instanz_profundum_slot");

            migrationBuilder.DropForeignKey(
                name: "fk_profundum_instanz_profundum_slot_profunda_slots_slots_id",
                table: "profundum_instanz_profundum_slot");

            migrationBuilder.DropForeignKey(
                name: "fk_profundum_profil_befreiungen_personen_betroffene_person_id",
                table: "profundum_profil_befreiungen");

            migrationBuilder.DropForeignKey(
                name: "fk_scheduled_emails_personen_recipient_id",
                table: "scheduled_emails");

            migrationBuilder.DropPrimaryKey(
                name: "pk_schultage",
                table: "schultage");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda",
                table: "profunda");

            migrationBuilder.DropPrimaryKey(
                name: "pk_personen",
                table: "personen");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otia",
                table: "otia");

            migrationBuilder.DropPrimaryKey(
                name: "pk_blocks",
                table: "blocks");

            migrationBuilder.DropPrimaryKey(
                name: "pk_scheduled_emails",
                table: "scheduled_emails");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundum_profil_befreiungen",
                table: "profundum_profil_befreiungen");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundum_instanz_profundum_slot",
                table: "profundum_instanz_profundum_slot");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundum_feedback_kategories",
                table: "profundum_feedback_kategories");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundum_feedback_entries",
                table: "profundum_feedback_entries");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundum_feedback_anker",
                table: "profundum_feedback_anker");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundum_fachbereich_profundum_feedback_kategorie",
                table: "profundum_fachbereich_profundum_feedback_kategorie");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundum_einwahl_zeitraeume",
                table: "profundum_einwahl_zeitraeume");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundum_definition_profundum_fachbereich",
                table: "profundum_definition_profundum_fachbereich");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundum_definition_dependencies",
                table: "profundum_definition_dependencies");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda_termine",
                table: "profunda_termine");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda_slots",
                table: "profunda_slots");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda_kategorien",
                table: "profunda_kategorien");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda_instanzen",
                table: "profunda_instanzen");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda_fachbereiche",
                table: "profunda_fachbereiche");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda_einschreibungen",
                table: "profunda_einschreibungen");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profunda_beleg_wuensche",
                table: "profunda_beleg_wuensche");

            migrationBuilder.DropPrimaryKey(
                name: "pk_person_profundum_instanz",
                table: "person_profundum_instanz");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otium_definition_person",
                table: "otium_definition_person");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otia_wiederholungen",
                table: "otia_wiederholungen");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otia_termine",
                table: "otia_termine");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otia_kategorien",
                table: "otia_kategorien");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otia_einschreibungs_notizen",
                table: "otia_einschreibungs_notizen");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otia_einschreibungen",
                table: "otia_einschreibungen");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otia_anwesenheiten",
                table: "otia_anwesenheiten");

            migrationBuilder.DropPrimaryKey(
                name: "pk_mentor_mentee_relations",
                table: "mentor_mentee_relations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_data_protection_keys",
                table: "data_protection_keys");

            migrationBuilder.DropPrimaryKey(
                name: "pk_calendar_subscriptions",
                table: "calendar_subscriptions");

            migrationBuilder.RenameTable(
                name: "schultage",
                newName: "Schultage");

            migrationBuilder.RenameTable(
                name: "profunda",
                newName: "Profunda");

            migrationBuilder.RenameTable(
                name: "personen",
                newName: "Personen");

            migrationBuilder.RenameTable(
                name: "otia",
                newName: "Otia");

            migrationBuilder.RenameTable(
                name: "blocks",
                newName: "Blocks");

            migrationBuilder.RenameTable(
                name: "scheduled_emails",
                newName: "ScheduledEmails");

            migrationBuilder.RenameTable(
                name: "profundum_profil_befreiungen",
                newName: "ProfundumProfilBefreiungen");

            migrationBuilder.RenameTable(
                name: "profundum_instanz_profundum_slot",
                newName: "ProfundumInstanzProfundumSlot");

            migrationBuilder.RenameTable(
                name: "profundum_feedback_kategories",
                newName: "ProfundumFeedbackKategories");

            migrationBuilder.RenameTable(
                name: "profundum_feedback_entries",
                newName: "ProfundumFeedbackEntries");

            migrationBuilder.RenameTable(
                name: "profundum_feedback_anker",
                newName: "ProfundumFeedbackAnker");

            migrationBuilder.RenameTable(
                name: "profundum_fachbereich_profundum_feedback_kategorie",
                newName: "ProfundumFachbereichProfundumFeedbackKategorie");

            migrationBuilder.RenameTable(
                name: "profundum_einwahl_zeitraeume",
                newName: "ProfundumEinwahlZeitraeume");

            migrationBuilder.RenameTable(
                name: "profundum_definition_profundum_fachbereich",
                newName: "ProfundumDefinitionProfundumFachbereich");

            migrationBuilder.RenameTable(
                name: "profundum_definition_dependencies",
                newName: "ProfundumDefinitionDependencies");

            migrationBuilder.RenameTable(
                name: "profunda_termine",
                newName: "ProfundaTermine");

            migrationBuilder.RenameTable(
                name: "profunda_slots",
                newName: "ProfundaSlots");

            migrationBuilder.RenameTable(
                name: "profunda_kategorien",
                newName: "ProfundaKategorien");

            migrationBuilder.RenameTable(
                name: "profunda_instanzen",
                newName: "ProfundaInstanzen");

            migrationBuilder.RenameTable(
                name: "profunda_fachbereiche",
                newName: "ProfundaFachbereiche");

            migrationBuilder.RenameTable(
                name: "profunda_einschreibungen",
                newName: "ProfundaEinschreibungen");

            migrationBuilder.RenameTable(
                name: "profunda_beleg_wuensche",
                newName: "ProfundaBelegWuensche");

            migrationBuilder.RenameTable(
                name: "person_profundum_instanz",
                newName: "PersonProfundumInstanz");

            migrationBuilder.RenameTable(
                name: "otium_definition_person",
                newName: "OtiumDefinitionPerson");

            migrationBuilder.RenameTable(
                name: "otia_wiederholungen",
                newName: "OtiaWiederholungen");

            migrationBuilder.RenameTable(
                name: "otia_termine",
                newName: "OtiaTermine");

            migrationBuilder.RenameTable(
                name: "otia_kategorien",
                newName: "OtiaKategorien");

            migrationBuilder.RenameTable(
                name: "otia_einschreibungs_notizen",
                newName: "OtiaEinschreibungsNotizen");

            migrationBuilder.RenameTable(
                name: "otia_einschreibungen",
                newName: "OtiaEinschreibungen");

            migrationBuilder.RenameTable(
                name: "otia_anwesenheiten",
                newName: "OtiaAnwesenheiten");

            migrationBuilder.RenameTable(
                name: "mentor_mentee_relations",
                newName: "MentorMenteeRelations");

            migrationBuilder.RenameTable(
                name: "data_protection_keys",
                newName: "DataProtectionKeys");

            migrationBuilder.RenameTable(
                name: "calendar_subscriptions",
                newName: "CalendarSubscriptions");

            migrationBuilder.RenameColumn(
                name: "wochentyp",
                table: "Schultage",
                newName: "Wochentyp");

            migrationBuilder.RenameColumn(
                name: "datum",
                table: "Schultage",
                newName: "Datum");

            migrationBuilder.RenameColumn(
                name: "bezeichnung",
                table: "Profunda",
                newName: "Bezeichnung");

            migrationBuilder.RenameColumn(
                name: "beschreibung",
                table: "Profunda",
                newName: "Beschreibung");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Profunda",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "min_klasse",
                table: "Profunda",
                newName: "MinKlasse");

            migrationBuilder.RenameColumn(
                name: "max_klasse",
                table: "Profunda",
                newName: "MaxKlasse");

            migrationBuilder.RenameColumn(
                name: "last_modified_by_id",
                table: "Profunda",
                newName: "LastModifiedById");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Profunda",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "kategorie_id",
                table: "Profunda",
                newName: "KategorieId");

            migrationBuilder.RenameColumn(
                name: "created_by_id",
                table: "Profunda",
                newName: "CreatedById");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Profunda",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_profunda_kategorie_id",
                table: "Profunda",
                newName: "IX_Profunda_KategorieId");

            migrationBuilder.RenameColumn(
                name: "rolle",
                table: "Personen",
                newName: "Rolle");

            migrationBuilder.RenameColumn(
                name: "gruppe",
                table: "Personen",
                newName: "Gruppe");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Personen",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Personen",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ldap_sync_time",
                table: "Personen",
                newName: "LdapSyncTime");

            migrationBuilder.RenameColumn(
                name: "ldap_sync_failure_time",
                table: "Personen",
                newName: "LdapSyncFailureTime");

            migrationBuilder.RenameColumn(
                name: "ldap_object_id",
                table: "Personen",
                newName: "LdapObjectId");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "Personen",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "global_permissions",
                table: "Personen",
                newName: "GlobalPermissions");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "Personen",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "bezeichnung",
                table: "Otia",
                newName: "Bezeichnung");

            migrationBuilder.RenameColumn(
                name: "beschreibung",
                table: "Otia",
                newName: "Beschreibung");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Otia",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "min_klasse",
                table: "Otia",
                newName: "MinKlasse");

            migrationBuilder.RenameColumn(
                name: "max_klasse",
                table: "Otia",
                newName: "MaxKlasse");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Otia",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "kategorie_id",
                table: "Otia",
                newName: "KategorieId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Otia",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_otia_kategorie_id",
                table: "Otia",
                newName: "IX_Otia_KategorieId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Blocks",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "sind_anwesenheiten_fehlernder_kontrolliert",
                table: "Blocks",
                newName: "SindAnwesenheitenFehlernderKontrolliert");

            migrationBuilder.RenameColumn(
                name: "schultag_key",
                table: "Blocks",
                newName: "SchultagKey");

            migrationBuilder.RenameColumn(
                name: "schema_id",
                table: "Blocks",
                newName: "SchemaId");

            migrationBuilder.RenameIndex(
                name: "ix_blocks_schultag_key_schema_id",
                table: "Blocks",
                newName: "IX_Blocks_SchultagKey_SchemaId");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "ScheduledEmails",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "deadline",
                table: "ScheduledEmails",
                newName: "Deadline");

            migrationBuilder.RenameColumn(
                name: "body",
                table: "ScheduledEmails",
                newName: "Body");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ScheduledEmails",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "recipient_id",
                table: "ScheduledEmails",
                newName: "RecipientId");

            migrationBuilder.RenameIndex(
                name: "ix_scheduled_emails_recipient_id",
                table: "ScheduledEmails",
                newName: "IX_ScheduledEmails_RecipientId");

            migrationBuilder.RenameColumn(
                name: "quartal",
                table: "ProfundumProfilBefreiungen",
                newName: "Quartal");

            migrationBuilder.RenameColumn(
                name: "jahr",
                table: "ProfundumProfilBefreiungen",
                newName: "Jahr");

            migrationBuilder.RenameColumn(
                name: "betroffene_person_id",
                table: "ProfundumProfilBefreiungen",
                newName: "BetroffenePersonId");

            migrationBuilder.RenameColumn(
                name: "slots_id",
                table: "ProfundumInstanzProfundumSlot",
                newName: "SlotsId");

            migrationBuilder.RenameColumn(
                name: "profundum_instanz_id",
                table: "ProfundumInstanzProfundumSlot",
                newName: "ProfundumInstanzId");

            migrationBuilder.RenameIndex(
                name: "ix_profundum_instanz_profundum_slot_slots_id",
                table: "ProfundumInstanzProfundumSlot",
                newName: "IX_ProfundumInstanzProfundumSlot_SlotsId");

            migrationBuilder.RenameColumn(
                name: "label",
                table: "ProfundumFeedbackKategories",
                newName: "Label");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ProfundumFeedbackKategories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "grad",
                table: "ProfundumFeedbackEntries",
                newName: "Grad");

            migrationBuilder.RenameColumn(
                name: "betroffene_person_id",
                table: "ProfundumFeedbackEntries",
                newName: "BetroffenePersonId");

            migrationBuilder.RenameColumn(
                name: "instanz_id",
                table: "ProfundumFeedbackEntries",
                newName: "InstanzId");

            migrationBuilder.RenameColumn(
                name: "anker_id",
                table: "ProfundumFeedbackEntries",
                newName: "AnkerId");

            migrationBuilder.RenameIndex(
                name: "ix_profundum_feedback_entries_instanz_id",
                table: "ProfundumFeedbackEntries",
                newName: "IX_ProfundumFeedbackEntries_InstanzId");

            migrationBuilder.RenameIndex(
                name: "ix_profundum_feedback_entries_betroffene_person_id",
                table: "ProfundumFeedbackEntries",
                newName: "IX_ProfundumFeedbackEntries_BetroffenePersonId");

            migrationBuilder.RenameColumn(
                name: "label",
                table: "ProfundumFeedbackAnker",
                newName: "Label");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ProfundumFeedbackAnker",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "kategorie_id",
                table: "ProfundumFeedbackAnker",
                newName: "KategorieId");

            migrationBuilder.RenameIndex(
                name: "ix_profundum_feedback_anker_kategorie_id",
                table: "ProfundumFeedbackAnker",
                newName: "IX_ProfundumFeedbackAnker_KategorieId");

            migrationBuilder.RenameColumn(
                name: "profundum_feedback_kategorie_id",
                table: "ProfundumFachbereichProfundumFeedbackKategorie",
                newName: "ProfundumFeedbackKategorieId");

            migrationBuilder.RenameColumn(
                name: "fachbereiche_id",
                table: "ProfundumFachbereichProfundumFeedbackKategorie",
                newName: "FachbereicheId");

            migrationBuilder.RenameIndex(
                name: "ix_profundum_fachbereich_profundum_feedback_kategorie_profundu~",
                table: "ProfundumFachbereichProfundumFeedbackKategorie",
                newName: "IX_ProfundumFachbereichProfundumFeedbackKategorie_ProfundumFee~");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ProfundumEinwahlZeitraeume",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "einwahl_stop",
                table: "ProfundumEinwahlZeitraeume",
                newName: "EinwahlStop");

            migrationBuilder.RenameColumn(
                name: "einwahl_start",
                table: "ProfundumEinwahlZeitraeume",
                newName: "EinwahlStart");

            migrationBuilder.RenameColumn(
                name: "profunda_id",
                table: "ProfundumDefinitionProfundumFachbereich",
                newName: "ProfundaId");

            migrationBuilder.RenameColumn(
                name: "fachbereiche_id",
                table: "ProfundumDefinitionProfundumFachbereich",
                newName: "FachbereicheId");

            migrationBuilder.RenameIndex(
                name: "ix_profundum_definition_profundum_fachbereich_profunda_id",
                table: "ProfundumDefinitionProfundumFachbereich",
                newName: "IX_ProfundumDefinitionProfundumFachbereich_ProfundaId");

            migrationBuilder.RenameColumn(
                name: "dependant_id",
                table: "ProfundumDefinitionDependencies",
                newName: "DependantId");

            migrationBuilder.RenameColumn(
                name: "dependency_id",
                table: "ProfundumDefinitionDependencies",
                newName: "DependencyId");

            migrationBuilder.RenameIndex(
                name: "ix_profundum_definition_dependencies_dependant_id",
                table: "ProfundumDefinitionDependencies",
                newName: "IX_ProfundumDefinitionDependencies_DependantId");

            migrationBuilder.RenameColumn(
                name: "day",
                table: "ProfundaTermine",
                newName: "Day");

            migrationBuilder.RenameColumn(
                name: "start_time",
                table: "ProfundaTermine",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "slot_id",
                table: "ProfundaTermine",
                newName: "SlotId");

            migrationBuilder.RenameColumn(
                name: "end_time",
                table: "ProfundaTermine",
                newName: "EndTime");

            migrationBuilder.RenameIndex(
                name: "ix_profunda_termine_slot_id",
                table: "ProfundaTermine",
                newName: "IX_ProfundaTermine_SlotId");

            migrationBuilder.RenameColumn(
                name: "wochentag",
                table: "ProfundaSlots",
                newName: "Wochentag");

            migrationBuilder.RenameColumn(
                name: "quartal",
                table: "ProfundaSlots",
                newName: "Quartal");

            migrationBuilder.RenameColumn(
                name: "jahr",
                table: "ProfundaSlots",
                newName: "Jahr");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ProfundaSlots",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "einwahl_zeitraum_id",
                table: "ProfundaSlots",
                newName: "EinwahlZeitraumId");

            migrationBuilder.RenameIndex(
                name: "ix_profunda_slots_einwahl_zeitraum_id",
                table: "ProfundaSlots",
                newName: "IX_ProfundaSlots_EinwahlZeitraumId");

            migrationBuilder.RenameColumn(
                name: "bezeichnung",
                table: "ProfundaKategorien",
                newName: "Bezeichnung");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ProfundaKategorien",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "profil_profundum",
                table: "ProfundaKategorien",
                newName: "ProfilProfundum");

            migrationBuilder.RenameColumn(
                name: "ort",
                table: "ProfundaInstanzen",
                newName: "Ort");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ProfundaInstanzen",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "profundum_id",
                table: "ProfundaInstanzen",
                newName: "ProfundumId");

            migrationBuilder.RenameColumn(
                name: "max_einschreibungen",
                table: "ProfundaInstanzen",
                newName: "MaxEinschreibungen");

            migrationBuilder.RenameColumn(
                name: "last_modified_by_id",
                table: "ProfundaInstanzen",
                newName: "LastModifiedById");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "ProfundaInstanzen",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "created_by_id",
                table: "ProfundaInstanzen",
                newName: "CreatedById");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ProfundaInstanzen",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_profunda_instanzen_profundum_id",
                table: "ProfundaInstanzen",
                newName: "IX_ProfundaInstanzen_ProfundumId");

            migrationBuilder.RenameColumn(
                name: "label",
                table: "ProfundaFachbereiche",
                newName: "Label");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ProfundaFachbereiche",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "profundum_instanz_id",
                table: "ProfundaEinschreibungen",
                newName: "ProfundumInstanzId");

            migrationBuilder.RenameColumn(
                name: "last_modified_by_id",
                table: "ProfundaEinschreibungen",
                newName: "LastModifiedById");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "ProfundaEinschreibungen",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_fixed",
                table: "ProfundaEinschreibungen",
                newName: "IsFixed");

            migrationBuilder.RenameColumn(
                name: "created_by_id",
                table: "ProfundaEinschreibungen",
                newName: "CreatedById");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ProfundaEinschreibungen",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "slot_id",
                table: "ProfundaEinschreibungen",
                newName: "SlotId");

            migrationBuilder.RenameColumn(
                name: "betroffene_person_id",
                table: "ProfundaEinschreibungen",
                newName: "BetroffenePersonId");

            migrationBuilder.RenameIndex(
                name: "ix_profunda_einschreibungen_slot_id",
                table: "ProfundaEinschreibungen",
                newName: "IX_ProfundaEinschreibungen_SlotId");

            migrationBuilder.RenameIndex(
                name: "ix_profunda_einschreibungen_profundum_instanz_id",
                table: "ProfundaEinschreibungen",
                newName: "IX_ProfundaEinschreibungen_ProfundumInstanzId");

            migrationBuilder.RenameColumn(
                name: "stufe",
                table: "ProfundaBelegWuensche",
                newName: "Stufe");

            migrationBuilder.RenameColumn(
                name: "einwahl_zeitraum_id",
                table: "ProfundaBelegWuensche",
                newName: "EinwahlZeitraumId");

            migrationBuilder.RenameColumn(
                name: "betroffene_person_id",
                table: "ProfundaBelegWuensche",
                newName: "BetroffenePersonId");

            migrationBuilder.RenameColumn(
                name: "profundum_instanz_id",
                table: "ProfundaBelegWuensche",
                newName: "ProfundumInstanzId");

            migrationBuilder.RenameIndex(
                name: "ix_profunda_beleg_wuensche_einwahl_zeitraum_id",
                table: "ProfundaBelegWuensche",
                newName: "IX_ProfundaBelegWuensche_EinwahlZeitraumId");

            migrationBuilder.RenameIndex(
                name: "ix_profunda_beleg_wuensche_betroffene_person_id",
                table: "ProfundaBelegWuensche",
                newName: "IX_ProfundaBelegWuensche_BetroffenePersonId");

            migrationBuilder.RenameColumn(
                name: "verantwortliche_id",
                table: "PersonProfundumInstanz",
                newName: "VerantwortlicheId");

            migrationBuilder.RenameColumn(
                name: "betreute_profunda_id",
                table: "PersonProfundumInstanz",
                newName: "BetreuteProfundaId");

            migrationBuilder.RenameIndex(
                name: "ix_person_profundum_instanz_verantwortliche_id",
                table: "PersonProfundumInstanz",
                newName: "IX_PersonProfundumInstanz_VerantwortlicheId");

            migrationBuilder.RenameColumn(
                name: "verwaltete_otia_id",
                table: "OtiumDefinitionPerson",
                newName: "VerwalteteOtiaId");

            migrationBuilder.RenameColumn(
                name: "verantwortliche_id",
                table: "OtiumDefinitionPerson",
                newName: "VerantwortlicheId");

            migrationBuilder.RenameIndex(
                name: "ix_otium_definition_person_verwaltete_otia_id",
                table: "OtiumDefinitionPerson",
                newName: "IX_OtiumDefinitionPerson_VerwalteteOtiaId");

            migrationBuilder.RenameColumn(
                name: "wochentyp",
                table: "OtiaWiederholungen",
                newName: "Wochentyp");

            migrationBuilder.RenameColumn(
                name: "wochentag",
                table: "OtiaWiederholungen",
                newName: "Wochentag");

            migrationBuilder.RenameColumn(
                name: "ort",
                table: "OtiaWiederholungen",
                newName: "Ort");

            migrationBuilder.RenameColumn(
                name: "block",
                table: "OtiaWiederholungen",
                newName: "Block");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OtiaWiederholungen",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tutor_id",
                table: "OtiaWiederholungen",
                newName: "TutorId");

            migrationBuilder.RenameColumn(
                name: "otium_id",
                table: "OtiaWiederholungen",
                newName: "OtiumId");

            migrationBuilder.RenameColumn(
                name: "max_einschreibungen",
                table: "OtiaWiederholungen",
                newName: "MaxEinschreibungen");

            migrationBuilder.RenameIndex(
                name: "ix_otia_wiederholungen_tutor_id",
                table: "OtiaWiederholungen",
                newName: "IX_OtiaWiederholungen_TutorId");

            migrationBuilder.RenameIndex(
                name: "ix_otia_wiederholungen_otium_id",
                table: "OtiaWiederholungen",
                newName: "IX_OtiaWiederholungen_OtiumId");

            migrationBuilder.RenameColumn(
                name: "ort",
                table: "OtiaTermine",
                newName: "Ort");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OtiaTermine",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "wiederholung_id",
                table: "OtiaTermine",
                newName: "WiederholungId");

            migrationBuilder.RenameColumn(
                name: "tutor_id",
                table: "OtiaTermine",
                newName: "TutorId");

            migrationBuilder.RenameColumn(
                name: "sind_anwesenheiten_kontrolliert",
                table: "OtiaTermine",
                newName: "SindAnwesenheitenKontrolliert");

            migrationBuilder.RenameColumn(
                name: "override_bezeichnung",
                table: "OtiaTermine",
                newName: "OverrideBezeichnung");

            migrationBuilder.RenameColumn(
                name: "override_beschreibung",
                table: "OtiaTermine",
                newName: "OverrideBeschreibung");

            migrationBuilder.RenameColumn(
                name: "otium_id",
                table: "OtiaTermine",
                newName: "OtiumId");

            migrationBuilder.RenameColumn(
                name: "max_einschreibungen",
                table: "OtiaTermine",
                newName: "MaxEinschreibungen");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "OtiaTermine",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "ist_abgesagt",
                table: "OtiaTermine",
                newName: "IstAbgesagt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "OtiaTermine",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "block_id",
                table: "OtiaTermine",
                newName: "BlockId");

            migrationBuilder.RenameIndex(
                name: "ix_otia_termine_wiederholung_id",
                table: "OtiaTermine",
                newName: "IX_OtiaTermine_WiederholungId");

            migrationBuilder.RenameIndex(
                name: "ix_otia_termine_tutor_id",
                table: "OtiaTermine",
                newName: "IX_OtiaTermine_TutorId");

            migrationBuilder.RenameIndex(
                name: "ix_otia_termine_otium_id",
                table: "OtiaTermine",
                newName: "IX_OtiaTermine_OtiumId");

            migrationBuilder.RenameIndex(
                name: "ix_otia_termine_block_id",
                table: "OtiaTermine",
                newName: "IX_OtiaTermine_BlockId");

            migrationBuilder.RenameColumn(
                name: "icon",
                table: "OtiaKategorien",
                newName: "Icon");

            migrationBuilder.RenameColumn(
                name: "bezeichnung",
                table: "OtiaKategorien",
                newName: "Bezeichnung");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OtiaKategorien",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "required_in",
                table: "OtiaKategorien",
                newName: "RequiredIn");

            migrationBuilder.RenameColumn(
                name: "parent_id",
                table: "OtiaKategorien",
                newName: "ParentId");

            migrationBuilder.RenameColumn(
                name: "ignore_enrollment_rule",
                table: "OtiaKategorien",
                newName: "IgnoreEnrollmentRule");

            migrationBuilder.RenameColumn(
                name: "css_color",
                table: "OtiaKategorien",
                newName: "CssColor");

            migrationBuilder.RenameIndex(
                name: "ix_otia_kategorien_parent_id",
                table: "OtiaKategorien",
                newName: "IX_OtiaKategorien_ParentId");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "OtiaEinschreibungsNotizen",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "OtiaEinschreibungsNotizen",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "OtiaEinschreibungsNotizen",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "author_id",
                table: "OtiaEinschreibungsNotizen",
                newName: "AuthorId");

            migrationBuilder.RenameColumn(
                name: "student_id",
                table: "OtiaEinschreibungsNotizen",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "block_id",
                table: "OtiaEinschreibungsNotizen",
                newName: "BlockId");

            migrationBuilder.RenameIndex(
                name: "ix_otia_einschreibungs_notizen_student_id",
                table: "OtiaEinschreibungsNotizen",
                newName: "IX_OtiaEinschreibungsNotizen_StudentId");

            migrationBuilder.RenameIndex(
                name: "ix_otia_einschreibungs_notizen_author_id",
                table: "OtiaEinschreibungsNotizen",
                newName: "IX_OtiaEinschreibungsNotizen_AuthorId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OtiaEinschreibungen",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "termin_id",
                table: "OtiaEinschreibungen",
                newName: "TerminId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "OtiaEinschreibungen",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "OtiaEinschreibungen",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "betroffene_person_id",
                table: "OtiaEinschreibungen",
                newName: "BetroffenePersonId");

            migrationBuilder.RenameIndex(
                name: "ix_otia_einschreibungen_termin_id",
                table: "OtiaEinschreibungen",
                newName: "IX_OtiaEinschreibungen_TerminId");

            migrationBuilder.RenameIndex(
                name: "ix_otia_einschreibungen_betroffene_person_id",
                table: "OtiaEinschreibungen",
                newName: "IX_OtiaEinschreibungen_BetroffenePersonId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "OtiaAnwesenheiten",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "student_id",
                table: "OtiaAnwesenheiten",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "block_id",
                table: "OtiaAnwesenheiten",
                newName: "BlockId");

            migrationBuilder.RenameIndex(
                name: "ix_otia_anwesenheiten_student_id",
                table: "OtiaAnwesenheiten",
                newName: "IX_OtiaAnwesenheiten_StudentId");

            migrationBuilder.RenameColumn(
                name: "student_id",
                table: "MentorMenteeRelations",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "mentor_id",
                table: "MentorMenteeRelations",
                newName: "MentorId");

            migrationBuilder.RenameIndex(
                name: "ix_mentor_mentee_relations_student_id",
                table: "MentorMenteeRelations",
                newName: "IX_MentorMenteeRelations_StudentId");

            migrationBuilder.RenameColumn(
                name: "xml",
                table: "DataProtectionKeys",
                newName: "Xml");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "DataProtectionKeys",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "friendly_name",
                table: "DataProtectionKeys",
                newName: "FriendlyName");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CalendarSubscriptions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "betroffene_person_id",
                table: "CalendarSubscriptions",
                newName: "BetroffenePersonId");

            migrationBuilder.RenameIndex(
                name: "ix_calendar_subscriptions_betroffene_person_id",
                table: "CalendarSubscriptions",
                newName: "IX_CalendarSubscriptions_BetroffenePersonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Schultage",
                table: "Schultage",
                column: "Datum");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Profunda",
                table: "Profunda",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Personen",
                table: "Personen",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Otia",
                table: "Otia",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Blocks",
                table: "Blocks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduledEmails",
                table: "ScheduledEmails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundumProfilBefreiungen",
                table: "ProfundumProfilBefreiungen",
                columns: new[] { "BetroffenePersonId", "Jahr", "Quartal" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundumInstanzProfundumSlot",
                table: "ProfundumInstanzProfundumSlot",
                columns: new[] { "ProfundumInstanzId", "SlotsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundumFeedbackKategories",
                table: "ProfundumFeedbackKategories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundumFeedbackEntries",
                table: "ProfundumFeedbackEntries",
                columns: new[] { "AnkerId", "InstanzId", "BetroffenePersonId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundumFeedbackAnker",
                table: "ProfundumFeedbackAnker",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundumFachbereichProfundumFeedbackKategorie",
                table: "ProfundumFachbereichProfundumFeedbackKategorie",
                columns: new[] { "FachbereicheId", "ProfundumFeedbackKategorieId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundumEinwahlZeitraeume",
                table: "ProfundumEinwahlZeitraeume",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundumDefinitionProfundumFachbereich",
                table: "ProfundumDefinitionProfundumFachbereich",
                columns: new[] { "FachbereicheId", "ProfundaId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundumDefinitionDependencies",
                table: "ProfundumDefinitionDependencies",
                columns: new[] { "DependencyId", "DependantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundaTermine",
                table: "ProfundaTermine",
                column: "Day");

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
                name: "PK_ProfundaFachbereiche",
                table: "ProfundaFachbereiche",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundaEinschreibungen",
                table: "ProfundaEinschreibungen",
                columns: new[] { "BetroffenePersonId", "SlotId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfundaBelegWuensche",
                table: "ProfundaBelegWuensche",
                columns: new[] { "ProfundumInstanzId", "BetroffenePersonId", "Stufe" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonProfundumInstanz",
                table: "PersonProfundumInstanz",
                columns: new[] { "BetreuteProfundaId", "VerantwortlicheId" });

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
                name: "FK_PersonProfundumInstanz_Personen_VerantwortlicheId",
                table: "PersonProfundumInstanz",
                column: "VerantwortlicheId",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonProfundumInstanz_ProfundaInstanzen_BetreuteProfundaId",
                table: "PersonProfundumInstanz",
                column: "BetreuteProfundaId",
                principalTable: "ProfundaInstanzen",
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
                name: "FK_ProfundaBelegWuensche_ProfundumEinwahlZeitraeume_EinwahlZei~",
                table: "ProfundaBelegWuensche",
                column: "EinwahlZeitraumId",
                principalTable: "ProfundumEinwahlZeitraeume",
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
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundaEinschreibungen_ProfundaSlots_SlotId",
                table: "ProfundaEinschreibungen",
                column: "SlotId",
                principalTable: "ProfundaSlots",
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
                name: "FK_ProfundaTermine_ProfundaSlots_SlotId",
                table: "ProfundaTermine",
                column: "SlotId",
                principalTable: "ProfundaSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundumDefinitionDependencies_Profunda_DependantId",
                table: "ProfundumDefinitionDependencies",
                column: "DependantId",
                principalTable: "Profunda",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundumDefinitionDependencies_Profunda_DependencyId",
                table: "ProfundumDefinitionDependencies",
                column: "DependencyId",
                principalTable: "Profunda",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundumDefinitionProfundumFachbereich_ProfundaFachbereich~",
                table: "ProfundumDefinitionProfundumFachbereich",
                column: "FachbereicheId",
                principalTable: "ProfundaFachbereiche",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundumDefinitionProfundumFachbereich_Profunda_ProfundaId",
                table: "ProfundumDefinitionProfundumFachbereich",
                column: "ProfundaId",
                principalTable: "Profunda",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundumFachbereichProfundumFeedbackKategorie_ProfundaFach~",
                table: "ProfundumFachbereichProfundumFeedbackKategorie",
                column: "FachbereicheId",
                principalTable: "ProfundaFachbereiche",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundumFachbereichProfundumFeedbackKategorie_ProfundumFee~",
                table: "ProfundumFachbereichProfundumFeedbackKategorie",
                column: "ProfundumFeedbackKategorieId",
                principalTable: "ProfundumFeedbackKategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundumFeedbackAnker_ProfundumFeedbackKategories_Kategori~",
                table: "ProfundumFeedbackAnker",
                column: "KategorieId",
                principalTable: "ProfundumFeedbackKategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundumFeedbackEntries_Personen_BetroffenePersonId",
                table: "ProfundumFeedbackEntries",
                column: "BetroffenePersonId",
                principalTable: "Personen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundumFeedbackEntries_ProfundaInstanzen_InstanzId",
                table: "ProfundumFeedbackEntries",
                column: "InstanzId",
                principalTable: "ProfundaInstanzen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfundumFeedbackEntries_ProfundumFeedbackAnker_AnkerId",
                table: "ProfundumFeedbackEntries",
                column: "AnkerId",
                principalTable: "ProfundumFeedbackAnker",
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
                name: "FK_ProfundumProfilBefreiungen_Personen_BetroffenePersonId",
                table: "ProfundumProfilBefreiungen",
                column: "BetroffenePersonId",
                principalTable: "Personen",
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
