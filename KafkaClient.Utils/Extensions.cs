using Confluent.Kafka;
using KafkaClient.Utils.Core;
using KafkaClient.Utils.Handlers;
using KafkaClient.Utils.Topology;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace KafkaClient.Utils;
public static class Extensions
{
    public static void AddKafka(this IServiceCollection services, IConfiguration kafkaConfigSection, params Assembly[] assemblies)
    {
        services.AddKafkaConfiguration(kafkaConfigSection);
        services.AddKafkaTopology(kafkaConfigSection);
        services.AddKafkaPublisher();
        services.AddKafkaConsumers(assemblies);
        services.AddKafkaClient();
    }

    public static void AddKafkaStartup(this IServiceCollection services, Func<IKafkaClient, IServiceProvider, Task> onStartup)
    {
        services.AddKeyedSingleton(StartupService.StartActionKey, onStartup);
        services.AddHostedService<StartupService>();
    }

    private const string CLIENT_SECTION = "Client";
    private const string MAPPING_SECTION = "Mapping";

    private static void AddKafkaConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ClientConfig>(configuration.GetSection(CLIENT_SECTION));
        services.AddSingleton<KafkaConsumerConfig>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<ClientConfig>>();
            var group = configuration[nameof(KafkaConsumerConfig.GroupId)]
                ?? throw new ArgumentException($"Failed to read config value: {nameof(KafkaConsumerConfig.GroupId)}");

            return new(config, group);
        });
        services.Configure<KafkaMappingConfig>(configuration.GetSection(MAPPING_SECTION));
    }

    private static void AddKafkaTopology(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<TopologyHandler>();
        services.AddSingleton<ITopology, TopologyHandler>(sp => sp.GetRequiredService<TopologyHandler>());
    }

    private static void AddKafkaConsumers(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddSingleton<Consumer>();
        services.AddSingleton<IConsumer, Consumer>(sp => sp.GetRequiredService<Consumer>());

        var scanning = assemblies
                .SelectMany(asm => asm.DefinedTypes
                .Where(t => t.IsAssignableTo(typeof(IAsyncHandler)) || t.IsAssignableTo(typeof(IHandler))));

        services.AddSingleton<IHandlerProvider>(sp => new HandlerProvider(scanning));

        foreach (var handler in scanning)
            services.AddScoped(handler);
    }
    private static void AddKafkaPublisher(this IServiceCollection services)
    {
        services.AddSingleton<Publisher>();
        services.AddSingleton<IPublisher, Publisher>(sp => sp.GetRequiredService<Publisher>());
    }
    private static void AddKafkaClient(this IServiceCollection services)
    {
        services.AddSingleton<IKafkaClient, Core.KafkaClient>();
    }
}
