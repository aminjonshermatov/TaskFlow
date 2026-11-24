using Microsoft.EntityFrameworkCore;

using TaskFlow.Contracts.Jobs;
using TaskFlow.Persistence;

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
      errors["Command"] = ["Command must not be empty."];
    }

    if (request.TimeoutSeconds <= 0)
    {
      errors["TimeoutSeconds"] = ["Max duration must be positive."];
    }

    RetryOn? retryOn = null;
    if (request.Retry is not null)
    {
      if (request.Retry.MaxAttempts <= 0)
      {
        errors["Retry.MaxAttempts"] = ["Max attempts must be positive."];
      }
      if (request.Retry.BackoffSeconds < 0)
      {
        errors["Retry.BackoffSeconds"] = ["Backoff must be non-negative."];
      }

      if (request.Retry.RetryOn is not null)
      {
        retryOn = RetryOn.None;
        foreach (var retryString in request.Retry.RetryOn)
        {
          var retry = ParseRetryOn(retryString);
          if (retry is not null)
          {
            if ((retryOn & retry).Value == retry)
            {
              errors["Retry.RetryOn"] = [$"Duplicate: {retryString}"];
              break;
            }
            retryOn |= retry.Value;
          }
          else
          {
            errors["Retry.RetryOn"] = [$"Invalid: {retryString}"];
            break;
          }
        }
      }
    }

    if (errors.Count > 0)
    {
      return Results.ValidationProblem(errors);
    }

    var command = new SubmitJobCommand(
      request.Command,
      request.StartAt,
      request.TimeoutSeconds,
      request.Retry?.MaxAttempts ?? 1,
      retryOn ?? RetryOn.Lost,
      request.Retry?.BackoffSeconds ?? 0);
    var job = await jobService.SubmitAsync(command, cancellationToken);

    return Results.Created(
        $"/api/jobs/{job.CorrelationId}",
        JobHttpResponse.FromDomain(job));
  }

  private static RetryOn? ParseRetryOn(string s) => s switch
  {
    "lost" => RetryOn.Lost,
    "failed" => RetryOn.Failed,
    "timed_out" => RetryOn.TimedOut,
    _ => null
  };

  private static async Task<IResult> GetByIdAsync(
      Guid jobId,
      TaskFlowDbContext dbContext,
      CancellationToken cancellationToken)
  {
    var job = await dbContext.Jobs
      .AsNoTracking()
      .Where(job => job.CorrelationId == jobId)
      .FirstOrDefaultAsync(cancellationToken);

    return job is null
        ? Results.NotFound()
        : Results.Ok(JobHttpResponse.FromDomain(job));
  }
}
