using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Migrator;

internal sealed class BackoffOptions
{
  public const string SectionName = "BackoffOptions";

  [Required]
  [Range(1, 10, ErrorMessage = "Positive integers <= 10")]
  public int MaxRetry { get; set; } = 5;

  [Required]
  [Range(1, 60 * 60, ErrorMessage = "Positive seconds not greater 1 hour")]
  public int InitialDelayInSeconds { get; set; } = 10;

  [Required]
  [Range(1, 5, ErrorMessage = "Positive integers <= 5")]
  public int DelayMultiplier { get; set; } = 2;

  [Required]
  [Range(1, 2 * 60 * 60, ErrorMessage = "Positive seconds not greater 2 hour")]
  public int TotalTimeoutSeconds { get; set; } = 161;
}
