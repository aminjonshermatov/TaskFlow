using Microsoft.Extensions.Options;

namespace TaskFlow.ServiceDefaults.Options;

[OptionsValidator]
internal sealed partial class ValidateRabbitMqOptions : IValidateOptions<RabbitMqOptions>
{
}
