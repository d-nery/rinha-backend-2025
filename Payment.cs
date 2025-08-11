using System.Text.Json.Serialization;

namespace rinha;

public sealed record Payment(Guid CorrelationId, decimal Amount);
public sealed record PaymentRequest(Guid CorrelationId, decimal Amount, DateTimeOffset RequestedAt);
public sealed record PaymentEntity(Guid CorrelationId, decimal Amount, string Processor, DateTimeOffset RequestedAt);

public sealed record PaymentSummary(Summary Default, Summary Fallback);

public record struct Summary(int TotalRequests, decimal TotalAmount)
{
    public Summary(SummaryEntity entity) : this(entity.TotalRequests, entity.TotalAmount) {}
}
public sealed record SummaryEntity(string Processor, int TotalRequests, decimal TotalAmount);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Payment))]
[JsonSerializable(typeof(PaymentSummary))]
[JsonSerializable(typeof(PaymentRequest))]
[JsonSerializable(typeof(Summary))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}