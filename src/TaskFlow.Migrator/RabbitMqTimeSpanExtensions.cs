namespace TaskFlow.Migrator;

public static class RabbitMqTimeSpanExtensions
{
  public static string ToRabbitMqDuration(this TimeSpan duration)
  {
    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duration, TimeSpan.Zero);
    if (Convert.ToUInt64(duration.TotalDays) >= 1 && duration.TotalDays % 1 == 0)
    {
      return $"{Convert.ToUInt64(duration.TotalDays)}D";
    }
    if (Convert.ToUInt64(duration.TotalHours) >= 1 && duration.TotalHours % 1 == 0)
    {
      return $"{Convert.ToUInt64(duration.TotalHours)}h";
    }
    if (Convert.ToUInt64(duration.TotalMinutes) >= 1 && duration.TotalMinutes % 1 == 0)
    {
      return $"{Convert.ToUInt64(duration.TotalMinutes)}m";
    }
    if (Convert.ToUInt64(duration.TotalSeconds) >= 1 && duration.TotalSeconds % 1 == 0)
    {
      return $"{Convert.ToUInt64(duration.TotalSeconds)}s";
    }

    var seconds = Math.Max(1, (Convert.ToUInt64(duration.TotalMilliseconds) + 999) / 1000);
    return $"{seconds}s";
  }
}
