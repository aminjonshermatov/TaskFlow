using Dapper;
using Npgsql;

namespace TaskFlow.Coordinator.Jobs;

public sealed class DapperJobRepository(NpgsqlDataSource dataSource) : IJobRepository
{
    private const string InsertSql = """
        insert into jobs (id, command, max_duration_seconds, status, worker_id, exit_code, created_at, started_at, finished_at)
        values (@Id, @Command, @MaxDurationSeconds, cast(@Status as job_status), @WorkerId, @ExitCode, @CreatedAt, @StartedAt, @FinishedAt)
        ;
        """;

    public async Task CreateAsync(Job job, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            InsertSql,
            new
            {
                job.Id,
                job.Command,
                job.MaxDurationSeconds,
                Status = job.Status.ToString().ToLowerInvariant(),
                job.WorkerId,
                job.ExitCode,
                job.CreatedAt,
                job.StartedAt,
                job.FinishedAt
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    private const string GetByIdSql = """
        select
          id as Id,
          command as Command,
          max_duration_seconds as MaxDurationSeconds,
          status as Status,
          worker_id as WorkerId,
          exit_code as ExitCode,
          created_at as CreatedAt,
          started_at as StartedAt,
          finished_at as FinishedAt
        from jobs
        where id = @JobId
        ;
        """;

    public async Task<Job?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            GetByIdSql,
            new { JobId = jobId },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<Job>(command);
    }
    
    private const string TryAssignNext = """
        with next_job as (
            select id
            from jobs
            where status = 'pending'
            order by created_at
            for update skip locked
            limit 1
        )
        update jobs as job set
            status = 'assigned'
            worker_id = @WorkerId
        from next_job
        where job.id = next_job.id
        returning
            job.id as Id,
            job.command as Command,
            job.max_duration_seconds as MaxDurationSeconds,
            job.status as Status,
            job.worker_id as WorkerId,
            job.exit_code as ExitCode,
            job.created_at as CreatedAt,
            job.started_at as StartedAt,
            job.finished_at as FinishedAt
        ;
        """;

    public async Task<Job?> TryAssignNextAsync(string workerId, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            TryAssignNext,
            new { WorkerId = workerId },
            cancellationToken: cancellationToken
        );
        
        return connection.QuerySingleOrDefault<Job?>(command);
    }
}