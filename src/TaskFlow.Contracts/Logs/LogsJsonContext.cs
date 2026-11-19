using System.Text.Json.Serialization;

namespace TaskFlow.Contracts.Logs;

[JsonSourceGenerationOptions(
  PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
  UseStringEnumConverter = true)]
[JsonSerializable(typeof(LogBatch))]
public sealed partial class LogsJsonContext : JsonSerializerContext;
