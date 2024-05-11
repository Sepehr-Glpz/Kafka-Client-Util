
using KafkaClient.Utils.Handlers;

namespace KafkaClient.Utils;
public interface IHandler
{
    HandleResult Handle(HandleArgs args);
}
