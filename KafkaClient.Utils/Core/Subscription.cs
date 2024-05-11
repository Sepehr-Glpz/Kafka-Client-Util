using System.Threading;
using System.Threading.Channels;

namespace KafkaClient.Utils.Core;
internal class Subscription : ISubscription
{
    #region Constructor

    public Subscription(KafkaConsumer consumer, CancellationTokenSource canceller, Action onCancel, Channel<KafkaMsgDelivery> nacks)
    {
        Canceller = canceller;
        Consumer = consumer;
        OnCancel = new(onCancel);
        OnNack = new((_, _) => { });
        _nackPiper = PipeNackEvents(nacks);
    }

    #endregion

    #region Fields

    protected CancellationTokenSource Canceller { get; }
    public KafkaConsumer Consumer { get; }
    public bool IsRunning => !Canceller.IsCancellationRequested;

    #endregion

    #region Events

    public event Action OnCancel;
    public event EventHandler<KafkaMsgDelivery> OnNack;

    private readonly Task _nackPiper;
    private async Task PipeNackEvents(Channel<KafkaMsgDelivery> nackChannel)
    {
        await foreach(var item in nackChannel.Reader.ReadAllAsync(this.Canceller.Token))
        {
            OnNack(this, item);
        }
    }

    #endregion

    #region Methods

    public void UnSubscribe()
    {
        if (IsRunning)
        {
            Canceller.Cancel();
            OnCancel();
        }
    }

    public void Dispose()
    {
        Canceller.Dispose();
    }

    #endregion
}