using Microsoft.Extensions.Options;

namespace TaskFlow.Persistence.Options;

[OptionsValidator]
internal sealed partial class ValidatePostgresOptions : IValidateOptions<PostgresOptions>
{
}
