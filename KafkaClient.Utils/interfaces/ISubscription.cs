
namespace KafkaClient.Utils;
public interface ISubscription : IDisposable
{
    KafkaConsumer Consumer { get; }

    bool IsRunning { get; }

    void UnSubscribe();

    event EventHandler<KafkaMsgDelivery> OnNack;
}
