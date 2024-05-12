using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace KafkaClient.Utils;
internal class StartupService(
    ILogger<StartupService> logger,
    IKafkaClient client, IServiceProvider serviceProvider,
    [FromKeyedServices(StartupService.StartActionKey)] Func<IKafkaClient, IServiceProvider, Task> startup) : IHostedService
{
    #region Deps
    public const string StartActionKey = "kafka-util-startup";
    private readonly ILogger<StartupService> _logger = logger;
    private readonly IKafkaClient _client = client;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly Func<IKafkaClient, IServiceProvider, Task> _startup = startup;
    #endregion

    #region Methods
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _startup(_client, _serviceProvider);
        }
        catch(Exception ex)
        {
            _logger
                .LogCritical(ex, "Failed to run akfka util startup action!");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    #endregion
}
