using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AllowFeedbackPerInstanceAndSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop existing constraints and indices that are being replaced
            migrationBuilder.DropForeignKey(
                name: "fk_profundum_feedback_entries_personen_betroffene_person_id",
                table: "profundum_feedback_entries");

            migrationBuilder.DropForeignKey(
                name: "fk_profundum_feedback_entries_profunda_instanzen_instanz_id",
                table: "profundum_feedback_entries");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profundum_feedback_entries",
                table: "profundum_feedback_entries");

            migrationBuilder.DropIndex(
                name: "ix_profundum_feedback_entries_betroffene_person_id",
                table: "profundum_feedback_entries");

            migrationBuilder.DropIndex(
                name: "ix_profundum_feedback_entries_instanz_id",
                table: "profundum_feedback_entries");

            // 2. Prepare the table for data migration
            // We rename the old column to keep the data accessible while we populate the new slot_id
            migrationBuilder.RenameColumn(
                name: "instanz_id",
                table: "profundum_feedback_entries",
                newName: "old_instanz_id");

            migrationBuilder.AddColumn<Guid>(
                name: "slot_id",
                table: "profundum_feedback_entries",
                type: "uuid",
                nullable: true);

            // 3. Migrate the data
            // Logic: Map (Person, Instance) to (Person, Slot) via the Enrollment table.
            // Selection: If multiple slots exist for an enrollment, prefer Q2 (Quartal == 2).
            // Otherwise, pick the first available slot.
            migrationBuilder.Sql(@"
                UPDATE ""profundum_feedback_entries"" f
                SET ""slot_id"" = (
                    SELECT e.""slot_id""
                    FROM ""profunda_einschreibungen"" e
                    INNER JOIN ""profunda_slots"" s ON e.""slot_id"" = s.""id""
                    WHERE e.""betroffene_person_id"" = f.""betroffene_person_id""
                      AND e.""profundum_instanz_id"" = f.""old_instanz_id""
                    ORDER BY
                        CASE WHEN s.""quartal"" = 2 THEN 0 ELSE 1 END,
                        s.""quartal"" ASC
                    LIMIT 1
                );
            ");

            // 4. Validation: Fail if feedback exists for a user/instance without an enrollment
            migrationBuilder.Sql(@"
                    DO $$
                    BEGIN
                        IF EXISTS (SELECT 1 FROM ""profundum_feedback_entries"" WHERE ""slot_id"" IS NULL) THEN
                            RAISE EXCEPTION 'Migration failed: Found feedback entry for a person/instance without a corresponding enrollment.';
                        END IF;
                    END $$;
            ");

            // 5. Finalize schema changes
            migrationBuilder.DropColumn(
                name: "old_instanz_id",
                table: "profundum_feedback_entries");

            migrationBuilder.AlterColumn<Guid>(
                name: "slot_id",
                table: "profundum_feedback_entries",
                type: "uuid",
                nullable: false,
                oldNullable: true);

            // Recreate PK with the new slot_id column
            migrationBuilder.AddPrimaryKey(
                name: "pk_profundum_feedback_entries",
                table: "profundum_feedback_entries",
                columns: new[] { "anker_id", "slot_id", "betroffene_person_id" });

            migrationBuilder.CreateIndex(
                name: "ix_profundum_feedback_entries_betroffene_person_id_slot_id",
                table: "profundum_feedback_entries",
                columns: new[] { "betroffene_person_id", "slot_id" });

            // Link feedback directly to the enrollment
            migrationBuilder.AddForeignKey(
                name: "fk_profundum_feedback_entries_profunda_einschreibungen_betroffe~",
                table: "profundum_feedback_entries",
                columns: new[] { "betroffene_person_id", "slot_id" },
                principalTable: "profunda_einschreibungen",
                principalColumns: new[] { "betroffene_person_id", "slot_id" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
