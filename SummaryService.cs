using System.Collections.Concurrent;
using System.Text;
using Dapper;
using Npgsql;

namespace rinha;

public static class SummaryService
{
    private static ConcurrentQueue<PaymentEntity> insertQueue = [];

    public static void RegisterPayment(PaymentEntity payment)
    {
        insertQueue.Enqueue(payment);
    }

    public static async Task AddPaymentsToDb()
    {
        if (insertQueue.IsEmpty)
        {
            return;
        }
        
        await using var dbConnection = new NpgsqlConnection("Host=postgres;Username=postgres;Password=postgres;Database=backend-db");
        await dbConnection.OpenAsync().ConfigureAwait(false);

        while (!insertQueue.IsEmpty)
        {
            var batch = new List<PaymentEntity>(50);
            while (batch.Count < 50 && insertQueue.TryDequeue(out var item))
            {
                batch.Add(item);
            }

            var parameters = new DynamicParameters();
            var sql = new StringBuilder();
            sql.AppendLine("INSERT INTO payments (correlation_id, amount, processor, requested_at) VALUES");

            for (var idx = 0; idx < batch.Count; idx++)
            {
                var i = idx.ToString();
                sql.Append($"(@CorrelationId{i}, @Amount{i}, @Processor{i}, @RequestedAt{i})");
                sql.AppendLine(idx < batch.Count - 1 ? "," : ";");

                parameters.Add($"CorrelationId{i}", batch[idx].CorrelationId);
                parameters.Add($"Amount{i}", batch[idx].Amount);
                parameters.Add($"Processor{i}", batch[idx].Processor);
                parameters.Add($"RequestedAt{i}", batch[idx].RequestedAt);
            }
    
            await dbConnection.ExecuteAsync(sql.ToString(), parameters).ConfigureAwait(false);
        }
    }

    public static async Task<PaymentSummary> GetPaymentSummary(DateTimeOffset? from, DateTimeOffset? to)
    {
        await using var dbConnection = new NpgsqlConnection("Host=postgres;Username=postgres;Password=postgres;Database=backend-db");
        await dbConnection.OpenAsync().ConfigureAwait(false);

        const string sql = """
            SELECT processor,
                COUNT(*) AS total_requests,
                SUM(amount) AS total_amount
            FROM payments
            WHERE (@from IS NULL OR requested_at >= @from)
            AND (@to IS NULL OR requested_at <= @to)
            GROUP BY processor;
        """;
        
        var result = (await dbConnection.QueryAsync<SummaryEntity>(sql, new { from, to }).ConfigureAwait(false)).ToList();
        
        var defaultSummary = result.FirstOrDefault(r => r.Processor == "default") ?? new SummaryEntity("default", 0, 0);
        var fallbackSummary = result.FirstOrDefault(r => r.Processor == "fallback") ?? new SummaryEntity("fallback", 0, 0);

        return new PaymentSummary(new Summary(defaultSummary), new Summary(fallbackSummary));
    }
}