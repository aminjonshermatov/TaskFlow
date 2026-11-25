using System.ComponentModel.DataAnnotations;

namespace TaskFlow.ServiceDefaults.Options;

public sealed class RabbitMqOptions
{
  public const string SectionName = "RabbitMq";

  [Required(AllowEmptyStrings = false)]
  public string Host { get; init; } = "";

  [Range(1, 65535, ErrorMessage = "Integers in range [1, 65535]")]
  public ushort Port { get; init; } = 5672;

  public string VHost { get; init; } = "/";

  [Required(AllowEmptyStrings = false)]
  public string Username { get; init; } = "";

  [Required(AllowEmptyStrings = false)]
  public string Password { get; init; } = "";

  [Range(5, 60, ErrorMessage = "Integers in range [5, 60]")]
  public int HeartBeatSeconds { get; init; } = 10;
}
