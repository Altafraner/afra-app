using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuartzTo4_6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DO $$
BEGIN
IF NOT EXISTS (SELECT 1 FROM information_schema.columns
              WHERE table_name = 'qrtz_scheduler_state' AND column_name = 'quartz_version') THEN
 ALTER TABLE qrtz_scheduler_state ADD COLUMN quartz_version varchar(20) null;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                 WHERE table_name = 'qrtz_fired_triggers' AND column_name = 'interrupt_requested_time') THEN
    ALTER TABLE qrtz_fired_triggers ADD COLUMN interrupt_requested_time bigint null;
  END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                 WHERE table_name = 'qrtz_triggers' AND column_name = 'misfire_threshold') THEN
    ALTER TABLE qrtz_triggers ADD COLUMN misfire_threshold bigint null;
  END IF;
END $$;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
