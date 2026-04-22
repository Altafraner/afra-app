using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class QuartzSchemaMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_triggers' AND column_name = 'misfire_orig_fire_time') THEN
                                     ALTER TABLE qrtz_triggers ADD COLUMN misfire_orig_fire_time bigint;
                                   END IF;
                                 END $$;
                                 """);

            migrationBuilder.Sql("""
                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_triggers' AND column_name = 'execution_group') THEN
                                     ALTER TABLE qrtz_triggers ADD COLUMN execution_group VARCHAR(200);
                                   END IF;
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_fired_triggers' AND column_name = 'execution_group') THEN
                                     ALTER TABLE qrtz_fired_triggers ADD COLUMN execution_group VARCHAR(200);
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
