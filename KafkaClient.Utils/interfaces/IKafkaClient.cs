
namespace KafkaClient.Utils;
public interface IKafkaClient
{
    IPublisher Publisher { get; }

    IConsumer Consumer { get; }
}
