namespace KafkaClient.Utils;
internal interface IHandlerProvider
{
    IEnumerable<IHandler> GetHandlers(string group, IServiceProvider serviceProvider);

    IEnumerable<IAsyncHandler> GetAsyncHandlers(string group, IServiceProvider serviceProvider);
}
