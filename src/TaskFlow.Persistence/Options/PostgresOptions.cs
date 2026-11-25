using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Persistence.Options;

public sealed class PostgresOptions
{
  public const string SectionName = "Postgres";

  [Required(AllowEmptyStrings = false)]
  public string ConnectionString { get; init; } = "";
}
