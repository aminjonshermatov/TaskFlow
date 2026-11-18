using TaskFlow.Coordinator.Jobs;

namespace TaskFlow.Coordinator.Jobs.Http;

public static class JobEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapJobEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/jobs")
            .WithTags("Jobs");

        group.MapPost("", SubmitAsync)
            .WithName("SubmitJob");

        group.MapGet("/{jobId:guid}", GetByIdAsync)
            .WithName("GetJob");

        return endpoints;
    }

    private static async Task<IResult> SubmitAsync(
        SubmitJobHttpRequest request,
        JobService jobService,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Command))
        {
            errors["command"] = ["Command must not be empty."];
        }

        if (request.MaxDurationSeconds <= 0)
        {
            errors["maxDurationSeconds"] =
                ["Max duration must be positive."];
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var job = await jobService.SubmitAsync(
            new SubmitJobCommand(
                request.Command,
                request.MaxDurationSeconds),
            cancellationToken);

        return Results.Created(
            $"/api/jobs/{job.Id}",
            JobHttpResponse.FromDomain(job));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid jobId,
        IJobRepository jobRepository,
        CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(
            jobId,
            cancellationToken);

        return job is null
            ? Results.NotFound()
            : Results.Ok(JobHttpResponse.FromDomain(job));
    }
}