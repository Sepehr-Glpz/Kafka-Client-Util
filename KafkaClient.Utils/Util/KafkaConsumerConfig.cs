using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace KafkaClient.Utils;
internal class KafkaConsumerConfig(IOptions<ClientConfig> config, string groupId)
{
    public ClientConfig ClientConfig { get; } = config.Value;

    public string GroupId { get; } = groupId;
}
