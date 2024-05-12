using System.Threading;

namespace KafkaClient.Utils;
public interface ITopology
{
    Task CommitAsync(CancellationToken ct = default);
}
