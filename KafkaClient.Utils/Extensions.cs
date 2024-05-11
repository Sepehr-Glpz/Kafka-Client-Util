using Confluent.Kafka;
using KafkaClient.Utils.Core;
using KafkaClient.Utils.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace KafkaClient.Utils;
public static class Extensions
{
    public static void AddKafka(this IServiceCollection services, IConfiguration rabbitConfiguration, params Assembly[] assemblies)
    {
        services.AddKafkaConfiguration(rabbitConfiguration);
        services.AddKafkaPublisher();
        services.AddKafkaConsumers(assemblies);
        services.AddKafkaClient();
    }

    private const string CLIENT_SECTION = "Client";

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
    }

    private static void AddKafkaConsumers(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddSingleton<IConsumer, Consumer>();

        var scanning = assemblies
                .SelectMany(asm => asm.DefinedTypes
                .Where(t => t.IsAssignableTo(typeof(IAsyncHandler)) || t.IsAssignableTo(typeof(IHandler))));

        services.AddSingleton<IHandlerProvider>(sp => new HandlerProvider(scanning));

        foreach (var handler in scanning)
            services.AddScoped(handler);
    }
    private static void AddKafkaPublisher(this IServiceCollection services)
    {
        services.AddSingleton<IPublisher, Publisher>();
    }
    private static void AddKafkaClient(this IServiceCollection services)
    {
        services.AddSingleton<IKafkaClient, Core.KafkaClient>();
    }
}
