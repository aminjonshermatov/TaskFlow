using TaskFlow.Migrator;

namespace TaskFlow.UnitTests.Migrator;

public sealed class RabbitMqTimeSpanTests
{
  public static TheoryData<TimeSpan, string> ValidDurations => new()
  {
    { TimeSpan.FromDays(1), "1D" },
    { TimeSpan.FromHours(36), "36h" },
    { TimeSpan.FromMinutes(90), "90m" },
    { TimeSpan.FromSeconds(45), "45s" },
    { TimeSpan.FromMilliseconds(1500), "2s" },
    { TimeSpan.FromMilliseconds(2_000_000_500), "2000001s" }
  };

  [Theory]
  [MemberData(nameof(ValidDurations))]
  public void ConvertToLargestWholeUnit(TimeSpan duration, string expected)
  {
    var actual = duration.ToRabbitMqDuration();
    Assert.Equal(expected, actual);
  }

  public static TheoryData<TimeSpan> InvalidDurations => new()
  {
    TimeSpan.Zero,
    TimeSpan.FromHours(-10),
    TimeSpan.FromDays(-2)
  };

  [Theory]
  [MemberData(nameof(InvalidDurations))]
  public void InvalidDurationsConversions(TimeSpan duration)
  {
    Assert.Throws<ArgumentOutOfRangeException>(() => duration.ToRabbitMqDuration());
  }
}
