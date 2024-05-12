using Confluent.Kafka;

namespace KafkaClient.Utils.Topology;
internal class KafkaMappingConfig
{
    public ClientConfig? Client { get; set; }
    public required IEnumerable<KafkaTopicConfig> Topics { get; set; }
}

internal class KafkaTopicConfig
{
    public required string Name { get; set; }

    public required uint PartitionCount { get; set; }

    public required uint ReplicationFactor { get; set; }

    public required Dictionary<string, string> Configs { get; set; }
}
