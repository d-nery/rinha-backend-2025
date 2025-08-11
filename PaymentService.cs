using System.Text.Json;

namespace rinha;

public static class PaymentService
{
    private static readonly HttpClient defaultClient;

    static PaymentService()
    {
        defaultClient = new HttpClient
        {
            BaseAddress = new Uri("http://payment-processor-default:8080"),
            // BaseAddress = new Uri("http://localhost:8001"),
        };
    }

    public static Task EnqueuePayment(Payment payment)
    {
        Queue.PushToPaymentQueue(payment);
        return Task.CompletedTask;
    }
    
    public static async Task Process(Payment payment)
    {
        var now = DateTimeOffset.UtcNow;
        var response = await defaultClient.PostAsJsonAsync(
            "/payments",
            new PaymentRequest(payment.CorrelationId, payment.Amount, now),
            AppJsonSerializerContext.Default.PaymentRequest).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            Queue.PushToPaymentQueue(payment);
            return;
        }

        SummaryService.RegisterPayment(new PaymentEntity(payment.CorrelationId, payment.Amount, "default", now));
    }
}