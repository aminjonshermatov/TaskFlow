using Microsoft.Extensions.Options;

namespace TaskFlow.Migrator;

[OptionsValidator]
internal sealed partial class ValidateBackoffOptions : IValidateOptions<BackoffOptions>
{
}
