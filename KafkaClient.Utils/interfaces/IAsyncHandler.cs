
using KafkaClient.Utils.Handlers;

namespace KafkaClient.Utils;

public interface IAsyncHandler
{
    Task<HandleResult> HandleAsync(HandleArgs args);
}
