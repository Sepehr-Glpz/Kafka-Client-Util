
namespace KafkaClient.Utils.Core;
internal class KafkaClient(IPublisher publisher, IConsumer consumer) : IKafkaClient
{
    public IPublisher Publisher { get; } = publisher;

    public IConsumer Consumer { get; } = consumer;
}
