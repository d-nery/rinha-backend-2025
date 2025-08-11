using System.Collections.Concurrent;

namespace rinha;

public static class Queue
{
    private static ConcurrentQueue<Payment> queue = new();

    public static void PushToPaymentQueue(Payment value)
    {
        queue.Enqueue(value);
    }

    public static Payment? GetFromPaymentQueue()
    {
        return queue.TryDequeue(out var value) ? value : null;
    }
}