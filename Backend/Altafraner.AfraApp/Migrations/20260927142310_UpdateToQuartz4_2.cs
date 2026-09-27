using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToQuartz4_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                 WHERE table_name = 'qrtz_triggers' AND column_name = 'preferred_node') THEN
    ALTER TABLE qrtz_triggers ADD COLUMN preferred_node varchar(200) null;
  END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                 WHERE table_name = 'qrtz_triggers' AND column_name = 'preferred_node_auto') THEN
    ALTER TABLE qrtz_triggers ADD COLUMN preferred_node_auto bool not null default false;
  END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                 WHERE table_name = 'qrtz_triggers' AND column_name = 'retry_policy') THEN
    ALTER TABLE qrtz_triggers ADD COLUMN retry_policy varchar(250) null;
  END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                 WHERE table_name = 'qrtz_triggers' AND column_name = 'retry_attempt') THEN
    ALTER TABLE qrtz_triggers ADD COLUMN retry_attempt integer null;
  END IF;
END $$;

CREATE TABLE IF NOT EXISTS qrtz_paused_job_grps (
  sched_name TEXT NOT NULL,
  job_group TEXT NOT NULL,
  PRIMARY KEY (sched_name, job_group)
);

DROP INDEX IF EXISTS idx_qrtz_t_nft_st;

CREATE INDEX IF NOT EXISTS idx_qrtz_j_g_n ON qrtz_job_details (sched_name, job_group, job_name);

CREATE INDEX IF NOT EXISTS idx_qrtz_t_j ON qrtz_triggers (sched_name, job_name, job_group);

CREATE INDEX IF NOT EXISTS idx_qrtz_t_g_n ON qrtz_triggers (sched_name, trigger_group, trigger_name);

CREATE INDEX IF NOT EXISTS idx_qrtz_t_c ON qrtz_triggers (sched_name, calendar_name);

CREATE INDEX IF NOT EXISTS idx_qrtz_t_nft_st ON qrtz_triggers (sched_name, trigger_state, next_fire_time asc, priority desc, misfire_instr);

CREATE INDEX IF NOT EXISTS idx_qrtz_ft_inst_job_req_rcvry ON qrtz_fired_triggers (sched_name, instance_name, requests_recovery);

CREATE INDEX IF NOT EXISTS idx_qrtz_ft_j_g ON qrtz_fired_triggers (sched_name, job_name, job_group);

CREATE INDEX IF NOT EXISTS idx_qrtz_ft_t_g ON qrtz_fired_triggers (sched_name, trigger_name, trigger_group);

DROP INDEX IF EXISTS idx_qrtz_j_grp;

DROP INDEX IF EXISTS idx_qrtz_j_req_recovery;

DROP INDEX IF EXISTS idx_qrtz_t_g_j;

DROP INDEX IF EXISTS idx_qrtz_t_jg;

DROP INDEX IF EXISTS idx_qrtz_t_g;

DROP INDEX IF EXISTS idx_qrtz_t_state;

DROP INDEX IF EXISTS idx_qrtz_t_n_state;

DROP INDEX IF EXISTS idx_qrtz_t_n_g_state;

DROP INDEX IF EXISTS idx_qrtz_t_next_fire_time;

DROP INDEX IF EXISTS idx_qrtz_t_nft_misfire;

DROP INDEX IF EXISTS idx_qrtz_t_nft_st_misfire_grp;

DROP INDEX IF EXISTS idx_qrtz_t_nft_st_misfire;

DROP INDEX IF EXISTS idx_qrtz_ft_g_j;

DROP INDEX IF EXISTS idx_qrtz_ft_g_t;

DROP INDEX IF EXISTS idx_qrtz_ft_jg;

DROP INDEX IF EXISTS idx_qrtz_ft_tg;

DROP INDEX IF EXISTS idx_qrtz_ft_trig_inst_name;

DROP INDEX IF EXISTS idx_qrtz_ft_trig_nm_gp;

DROP INDEX IF EXISTS idx_qrtz_ft_trig_name;

DROP INDEX IF EXISTS idx_qrtz_ft_trig_group;

DROP INDEX IF EXISTS idx_qrtz_ft_job_name;

DROP INDEX IF EXISTS idx_qrtz_ft_job_group;

DROP INDEX IF EXISTS idx_qrtz_ft_job_req_recovery;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                 WHERE table_name = 'qrtz_triggers' AND column_name = 'continues_trigger_name') THEN
    ALTER TABLE qrtz_triggers ADD COLUMN continues_trigger_name text null;
  END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                 WHERE table_name = 'qrtz_triggers' AND column_name = 'continues_trigger_group') THEN
    ALTER TABLE qrtz_triggers ADD COLUMN continues_trigger_group text null;
  END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                 WHERE table_name = 'qrtz_triggers' AND column_name = 'continuation_condition') THEN
    ALTER TABLE qrtz_triggers ADD COLUMN continuation_condition integer null;
  END IF;
END $$;

CREATE TABLE IF NOT EXISTS qrtz_execution_history (
  sched_name text not null,
  entry_id text not null,
  instance_name text not null,
  job_name text not null,
  job_group text not null,
  trigger_name text not null,
  trigger_group text not null,
  fired_time bigint not null,
  run_time bigint not null,
  succeeded bool not null,
  error_message text null,
  retry_attempt integer not null default 0,
  retry_scheduled bool not null default false,
  primary key (sched_name,entry_id)
);

CREATE TABLE IF NOT EXISTS qrtz_misfire_history (
  sched_name text not null,
  entry_id text not null,
  instance_name text not null,
  trigger_name text not null,
  trigger_group text not null,
  job_name text null,
  job_group text null,
  misfire_time bigint not null,
  sched_time bigint null,
  primary key (sched_name,entry_id)
);

CREATE INDEX IF NOT EXISTS idx_qrtz_eh_fired_time ON qrtz_execution_history (sched_name, fired_time);

CREATE INDEX IF NOT EXISTS idx_qrtz_eh_inst ON qrtz_execution_history (sched_name, instance_name);

CREATE INDEX IF NOT EXISTS idx_qrtz_mh_misfire_time ON qrtz_misfire_history (sched_name, misfire_time);

CREATE INDEX IF NOT EXISTS idx_qrtz_mh_inst ON qrtz_misfire_history (sched_name, instance_name);
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
