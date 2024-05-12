
namespace KafkaClient.Utils.Core;
internal class KafkaClient(IPublisher publisher, IConsumer consumer, ITopology topology) : IKafkaClient
{
    public IPublisher Publisher { get; } = publisher;

    public IConsumer Consumer { get; } = consumer;

    public ITopology Topology { get; } = topology;
}
