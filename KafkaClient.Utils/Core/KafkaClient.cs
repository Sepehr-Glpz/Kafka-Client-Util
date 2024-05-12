
namespace KafkaClient.Utils.Core;
internal class KafkaClient(Publisher publisher, Consumer consumer, TopologyHandler topology) : IKafkaClient
{
    private readonly Publisher _publisher = publisher;
    public IPublisher Publisher => _publisher;

    private readonly Consumer _consumer = consumer;
    public IConsumer Consumer => _consumer;

    private readonly TopologyHandler _topologyHandler = topology;
    public ITopology Topology => _topologyHandler;

    public void EnableTransactions() => _publisher.Producer.InitTransactions(TimeSpan.FromSeconds(5));
}
