using System.Text.Json;

using TaskFlow.Contracts.Logs;

namespace TaskFlow.UnitTests.Contracts.Logs;

public sealed class LogBatchJsonTests
{
  private static readonly DateTimeOffset At = new(2026, 11, 19, 10, 30, 15, 123, TimeSpan.FromHours(3));

  private static LogBatch Sample() => new(
    JobId: Guid.CreateVersion7(),
    Attempt: 2,
    FirstSeq: 123,
    Lines:
    [
      new LogLine(LogStream.Stdout, At, "successfully finished"),
      new LogLine(LogStream.Stderr, At.AddMilliseconds(5), "file X not found"),
    ]);

  [Fact]
  public void TestAllFields()
  {
    var original = Sample();

    var json = JsonSerializer.Serialize(original, LogsJsonContext.Default.LogBatch);
    var copy = JsonSerializer.Deserialize(json, LogsJsonContext.Default.LogBatch);

    Assert.NotNull(copy);
    Assert.Equal(original.JobId, copy.JobId);
    Assert.Equal(original.Attempt, copy.Attempt);
    Assert.Equal(original.FirstSeq, copy.FirstSeq);
    Assert.Equal(original.Lines, copy.Lines);
  }

  [Fact]
  public void TestTimeOffset()
  {
    var copy = JsonSerializer.Deserialize(
      JsonSerializer.Serialize(Sample(), LogsJsonContext.Default.LogBatch),
      LogsJsonContext.Default.LogBatch);

    Assert.NotNull(copy);
    Assert.Equal(At.Offset, copy.Lines[0].Ts.Offset);
  }
}
