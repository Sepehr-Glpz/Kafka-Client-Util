
namespace KafkaClient.Utils.Handlers;
public enum HandleResult
{
    Undefined = 0,
    Ack = 1,
    Nack = 2,
    Requeue = 4,
}
