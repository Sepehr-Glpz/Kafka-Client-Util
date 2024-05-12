
namespace KafkaClient.Utils;
public interface IKafkaClient
{
    void EnableTransactions();

    IPublisher Publisher { get; }

    IConsumer Consumer { get; }

    ITopology Topology { get; }
}
