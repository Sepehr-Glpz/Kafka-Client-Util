
using Confluent.Kafka;

namespace KafkaClient.Utils;
public interface IConsumer
{
    ISubscription Consume(string group, string topic, ConsumerConfig? config = null);
}
