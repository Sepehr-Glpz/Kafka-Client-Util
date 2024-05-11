using Confluent.Kafka;
using System.Threading;

namespace KafkaClient.Utils;
internal static class Utility
{
    public static IEnumerable<ConsumeResult<TKey, TMessage>> Iterate<TKey, TMessage>(this IConsumer<TKey, TMessage> consumer, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            ConsumeResult<TKey, TMessage> res;

            try
            {
                res = consumer.Consume(ct);
            }
            catch
            {
                continue;
            }

            yield return res;
        }
    }
}
