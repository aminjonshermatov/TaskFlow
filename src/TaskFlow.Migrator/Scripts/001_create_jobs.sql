create type job_status as enum ('pending', 'running', 'canceled', 'timeout', 'success');

create table jobs (
  id uuid primary key,
  command text not null,
  max_duration_seconds integer not null check (max_duration_seconds > 0),

  status job_status not null,

  worker_id varchar(256) null,
  exit_code integer null,

  created_at timestamptz not null,
  started_at timestamptz null,
  finished_at timestamptz null
);
