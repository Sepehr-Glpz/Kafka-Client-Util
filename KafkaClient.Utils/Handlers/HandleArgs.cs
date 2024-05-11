using Confluent.Kafka;
using System.Threading;

namespace KafkaClient.Utils.Handlers;
public readonly struct HandleArgs(KafkaMsg delivery, ISerializer serializer)
{
    private readonly KafkaMsg _delivery = delivery;

    private readonly ISerializer _serializer = serializer;

    public string GetKey() => _delivery.Key;
    public Headers GetHeaders() => _delivery.Headers;
    public TMessage? BodyAs<TMessage>() => _serializer.Deserialize<TMessage>(_delivery.Value);
    public Task BodyAsAsync<TMessage>(CancellationToken ct) => _serializer.DeserializeAsync<TMessage>(_delivery.Value, ct);
    public byte[] BodyRaw() => [.._delivery.Value];
}
