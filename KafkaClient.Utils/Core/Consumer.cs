using Confluent.Kafka;
using KafkaClient.Utils.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;

namespace KafkaClient.Utils.Core;
internal class Consumer : IConsumer
{
    #region Constructor

    public Consumer(IHandlerProvider handlerProvider, KafkaConsumerConfig config, IServiceProvider serviceProvider)
    {
        _provider = serviceProvider;
        Provider = handlerProvider;
        RunningConsumers = [];
        Config = CreateDefaultConfig(config);
    }
    
    #endregion

    #region Dependencies

    private readonly IServiceProvider _provider;
    protected IHandlerProvider Provider { get; }

    #endregion

    #region Fields

    public ISerializer Serializer { get; set; } = JsonMessageSerializer.Instance;
    protected ConcurrentDictionary<Guid, Task> RunningConsumers { get; }
    public ConsumerConfig Config { get; }

    #endregion

    #region Methods

    public ISubscription Consume(string group, string topic, ConsumerConfig? config = null)
    {
        var consumer = CreateConsumer(config switch
        {
            not null => MergeConfigs(config),
            _ => this.Config,
        });

        consumer.Subscribe(topic);

        var consumerId = Guid.NewGuid();
        var source = new CancellationTokenSource();
        var nacksChannel = Channel.CreateUnbounded<KafkaMsgDelivery>();

        RunningConsumers[consumerId] = Task.Factory.StartNew(() => RunConsumer(group, consumer, nacksChannel, source.Token));

        return new Subscription(
            consumer,
            source,
            () => RunningConsumers.Remove(consumerId, out _),
            nacksChannel);
    }

    #endregion

    #region Protected Util

    protected virtual async Task RunConsumer(string group, KafkaConsumer consumer, Channel<KafkaMsgDelivery> nacks, CancellationToken ct)
    {
        foreach (KafkaMsgDelivery msg in consumer.Iterate(ct))
        {
            try
            {
                using var scope = _provider.CreateScope();

                var syncHandlers = Provider.GetHandlers(group, scope.ServiceProvider);
                var asyncHandlers = Provider.GetAsyncHandlers(group, scope.ServiceProvider);

                if (!(syncHandlers.Any() || asyncHandlers.Any()))
                    continue;

                var syncTasks = 
                    syncHandlers
                    .Select(s => Task.Factory.StartNew(() => s.Handle(new HandleArgs(msg.Message, Serializer))));
                var asyncTasks =
                    asyncHandlers
                    .Select(s => s.HandleAsync(new HandleArgs(msg.Message, Serializer)));

                var allTasks = Enumerable.Union(syncTasks, asyncTasks);

                var handled = (await Task.WhenAll(allTasks))
                    .Aggregate(HandleResult.Undefined, (prev, current) => prev | current);

                if ((handled & HandleResult.Ack) == HandleResult.Ack)
                {
                    consumer.Commit(msg);
                }
                else if ((handled & HandleResult.Nack) == HandleResult.Nack)
                {
                    await nacks.Writer.WriteAsync(msg, ct);

                    if ((handled & HandleResult.Requeue) == HandleResult.Requeue)
                        consumer.Seek(msg.TopicPartitionOffset);
                    else
                        consumer.Commit(msg);
                }
            }
            catch
            {
                continue;
            }
        }
    }

    protected virtual KafkaConsumer CreateConsumer(ConsumerConfig config) =>
        new ConsumerBuilder<string, byte[]>(config.ToDictionary())
        .SetKeyDeserializer(Deserializers.Utf8)
        .SetValueDeserializer(Deserializers.ByteArray)
        .Build();

    #endregion

    #region Protecetd Utils

    protected virtual ConsumerConfig CreateDefaultConfig(KafkaConsumerConfig config) =>
        new(config.ClientConfig)
        {
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AutoCommitIntervalMs = 0,
            EnableAutoOffsetStore = false,
            GroupId = config.GroupId,
            IsolationLevel = IsolationLevel.ReadCommitted,
            PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky,
        };

    public virtual ConsumerConfig MergeConfigs(ConsumerConfig config)
    {
        var dict = new Dictionary<string, string>(Config);

        foreach (var (key, val) in config)
        {
            dict[key] = val;
        }

        return new ConsumerConfig(dict);
    }

    #endregion
}
