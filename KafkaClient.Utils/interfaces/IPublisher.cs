using Confluent.Kafka;
using System.Threading;

namespace KafkaClient.Utils;
public interface IPublisher
{
    ISerializer Serializer { get; set; }

    void Publish<TMessage>(string topic, string key, TMessage message, IReadOnlyDictionary<string, string>? headers, Action<string, ErrorCode> onFail);
    Task<bool> PublishAsync<TMessage>(string topic, string key, TMessage message, IReadOnlyDictionary<string, string>? headers = null, CancellationToken ct = default);

    void StartTransaction();
    void CommitTransaction();
    void RollbackTransaction();
}
