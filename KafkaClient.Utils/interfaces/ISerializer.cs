using System.Threading;

namespace KafkaClient.Utils;
public interface ISerializer
{
    byte[] Serialize<TMessage>(TMessage message);
    Task<byte[]> SerializeAsync<TMessage>(TMessage message, CancellationToken ct = default);

    TMessage? Deserialize<TMessage>(byte[] data);
    Task<TMessage?> DeserializeAsync<TMessage>(byte[] data, CancellationToken ct = default);
}
