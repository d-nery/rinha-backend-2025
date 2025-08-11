namespace rinha;

public class PaymentHandler : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.WhenAll(Process(stoppingToken), Process(stoppingToken), Process(stoppingToken), AddToDb(stoppingToken));
    }

    private static async Task Process(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var value = Queue.GetFromPaymentQueue();
                if (value == null)
                {
                    await Task.Delay(1, token).ConfigureAwait(false);
                    continue;
                }
                await PaymentService.Process(value).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    private static async Task AddToDb(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await SummaryService.AddPaymentsToDb().ConfigureAwait(false);
                await Task.Delay(20, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}