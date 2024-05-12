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

    public static ClientConfig MergeConfigs(this ClientConfig main, params ClientConfig[] others)
    {
        var result = new Dictionary<string, string>(main.ToDictionary());
        foreach (var config in others)
        {
            foreach(var (key, val) in config)
            {
                result[key] = val;
            }
        }

        return new ClientConfig(result);
    }

}
