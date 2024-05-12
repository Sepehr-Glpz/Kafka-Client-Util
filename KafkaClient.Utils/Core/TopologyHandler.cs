using Confluent.Kafka;
using Confluent.Kafka.Admin;
using KafkaClient.Utils.Topology;
using Microsoft.Extensions.Options;
using System.Threading;

namespace KafkaClient.Utils.Core;
internal class TopologyHandler : ITopology
{
    #region Deps

    protected KafkaMappingConfig Mappings { get; }

    protected AdminClientConfig Config { get; }

    protected IAdminClient Client { get; }

    #endregion

    #region Constructor

    public TopologyHandler(IOptions<KafkaMappingConfig> mappings, IOptions<ClientConfig> config)
    {
        Mappings = mappings.Value;
        Config = CreateAdminConfig(mappings.Value.Client switch
        {
            
            not null => mappings.Value.Client.MergeConfigs(config.Value),
            _ => config.Value,
        });
        Client = CreateAdminClient();
    }

    #endregion

    #region Methods

    public async Task CommitAsync(CancellationToken ct = default)
    {
        var nonExistentTopics = GetNonExistentTopics(Mappings.Topics);

        await Client.CreateTopicsAsync(nonExistentTopics.Select(s => new TopicSpecification()
        {
            Name = s.Name,
            Configs = s.Configs,
            NumPartitions = (int)s.PartitionCount,
            ReplicationFactor = (short)s.ReplicationFactor,
        }));
    }

    #endregion

    #region Portected Util

    protected virtual IEnumerable<KafkaTopicConfig> GetNonExistentTopics(IEnumerable<KafkaTopicConfig> configs)
    {
        var meta = Client.GetMetadata(TimeSpan.FromSeconds(5));

        return configs.Where(c => !meta.Topics.Any(x => x.Topic.Equals(c.Name, StringComparison.OrdinalIgnoreCase)));
    }

    protected virtual IAdminClient CreateAdminClient() =>
        new AdminClientBuilder(Config)
        .Build();

    protected virtual AdminClientConfig CreateAdminConfig(ClientConfig config) => 
        new(config.ToDictionary())
        {
        };

    #endregion
}
