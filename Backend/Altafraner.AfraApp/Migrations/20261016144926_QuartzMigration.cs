using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class QuartzMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_fired_triggers' AND column_name = 'progress') THEN
                                     ALTER TABLE qrtz_fired_triggers ADD COLUMN progress integer null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_fired_triggers' AND column_name = 'progress_message') THEN
                                     ALTER TABLE qrtz_fired_triggers ADD COLUMN progress_message varchar(250) null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_triggers' AND column_name = 'overlap_policy') THEN
                                     ALTER TABLE qrtz_triggers ADD COLUMN overlap_policy integer null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_triggers' AND column_name = 'pause_reason') THEN
                                     ALTER TABLE qrtz_triggers ADD COLUMN pause_reason varchar(250) null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_triggers' AND column_name = 'paused_by') THEN
                                     ALTER TABLE qrtz_triggers ADD COLUMN paused_by varchar(200) null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_triggers' AND column_name = 'paused_at') THEN
                                     ALTER TABLE qrtz_triggers ADD COLUMN paused_at bigint null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_paused_trigger_grps' AND column_name = 'pause_reason') THEN
                                     ALTER TABLE qrtz_paused_trigger_grps ADD COLUMN pause_reason varchar(250) null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_paused_trigger_grps' AND column_name = 'paused_by') THEN
                                     ALTER TABLE qrtz_paused_trigger_grps ADD COLUMN paused_by varchar(200) null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_paused_trigger_grps' AND column_name = 'paused_at') THEN
                                     ALTER TABLE qrtz_paused_trigger_grps ADD COLUMN paused_at bigint null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_paused_job_grps' AND column_name = 'pause_reason') THEN
                                     ALTER TABLE qrtz_paused_job_grps ADD COLUMN pause_reason varchar(250) null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_paused_job_grps' AND column_name = 'paused_by') THEN
                                     ALTER TABLE qrtz_paused_job_grps ADD COLUMN paused_by varchar(200) null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_paused_job_grps' AND column_name = 'paused_at') THEN
                                     ALTER TABLE qrtz_paused_job_grps ADD COLUMN paused_at bigint null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_execution_history' AND column_name = 'execution_log') THEN
                                     ALTER TABLE qrtz_execution_history ADD COLUMN execution_log text null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_misfire_history' AND column_name = 'reason') THEN
                                     ALTER TABLE qrtz_misfire_history ADD COLUMN reason integer null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_execution_history' AND column_name = 'result') THEN
                                     ALTER TABLE qrtz_execution_history ADD COLUMN result integer null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_execution_history' AND column_name = 'summary') THEN
                                     ALTER TABLE qrtz_execution_history ADD COLUMN summary text null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_execution_history' AND column_name = 'metrics') THEN
                                     ALTER TABLE qrtz_execution_history ADD COLUMN metrics text null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_execution_history' AND column_name = 'manual') THEN
                                     ALTER TABLE qrtz_execution_history ADD COLUMN manual bool null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_execution_history' AND column_name = 'fire_instance_id') THEN
                                     ALTER TABLE qrtz_execution_history ADD COLUMN fire_instance_id text null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_execution_history' AND column_name = 'job_input') THEN
                                     ALTER TABLE qrtz_execution_history ADD COLUMN job_input text null;
                                   END IF;
                                 END $$;

                                 DO $$
                                 BEGIN
                                   IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                                  WHERE table_name = 'qrtz_execution_history' AND column_name = 'job_input_too_large') THEN
                                     ALTER TABLE qrtz_execution_history ADD COLUMN job_input_too_large bool null;
                                   END IF;
                                 END $$;

                                 CREATE INDEX IF NOT EXISTS idx_qrtz_eh_job_time ON qrtz_execution_history (sched_name, job_group, job_name, fired_time);

                                 CREATE TABLE IF NOT EXISTS qrtz_job_status (
                                   sched_name text not null,
                                   job_group text not null,
                                   job_name text not null,
                                   first_fired_time bigint not null,
                                   last_fired_time bigint not null,
                                   last_result integer not null,
                                   last_run_time bigint not null,
                                   last_instance_name text not null,
                                   last_entry_id text null,
                                   last_summary text null,
                                   last_success_time bigint null,
                                   last_failure_time bigint null,
                                   last_failure_message text null,
                                   consecutive_failures integer not null default 0,
                                   run_count bigint not null default 0,
                                   failure_count bigint not null default 0,
                                   primary key (sched_name,job_group,job_name)
                                 );
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
