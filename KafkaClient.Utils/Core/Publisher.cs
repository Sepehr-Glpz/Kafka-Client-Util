using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Threading;

namespace KafkaClient.Utils.Core;
internal class Publisher : IPublisher
{
    #region Constructor

    public Publisher(IOptions<ClientConfig> config)
    {
        Config = CreateConfig(config.Value);
        Producer = CreateProducer();
        Serializer = JsonMessageSerializer.Instance;
    }

    #endregion

    #region Fields

    protected ProducerConfig Config { get; }
    protected KafkaPublisher Producer { get; }

    public ISerializer Serializer { get; set; }

    #endregion

    #region Methods

    public void Publish<TMessage>(string topic, string key, TMessage message, IReadOnlyDictionary<string, string>? headers, Action<string, ErrorCode> onFail)
    {
        var body = Serializer.Serialize(message);

        var paylaod = new Message<string, byte[]>()
        {
            Headers = headers?.ToHeaders(),
            Key = key,
            Timestamp = Timestamp.Default,
            Value = body,
        };

        Producer.Produce(topic, paylaod, del =>
        {
            if(del.Error.IsError)
            {
                onFail(del.Key, del.Error.Code);
            }
        });
    }

    public async Task<bool> PublishAsync<TMessage>(string topic, string key, TMessage message, IReadOnlyDictionary<string, string>? headers = null, CancellationToken ct = default)
    {
        var body = await Serializer.SerializeAsync(message, ct);
        var payload = new KafkaMsg()
        {
            Key = key,
            Value = body,
            Headers = headers?.ToHeaders(),
            Timestamp = Timestamp.Default,
        };

        var del = await Producer.ProduceAsync(topic, payload, ct);

        return del.Status == PersistenceStatus.Persisted;
    }

    #endregion

    #region Private Methods

    protected virtual KafkaPublisher CreateProducer() =>
        new ProducerBuilder<string, byte[]>(Config)
        .SetValueSerializer(Serializers.ByteArray)
        .SetKeySerializer(Serializers.Utf8)
        .Build();

    protected virtual ProducerConfig CreateConfig(ClientConfig baseConfig) =>
        new(baseConfig.ToDictionary())
        {
            Partitioner = Partitioner.Consistent,
            EnableBackgroundPoll = true,
            EnableDeliveryReports = true,
            CompressionType = CompressionType.Zstd,
            CompressionLevel = 5,
            EnableIdempotence = true,
        };

    #endregion
}
