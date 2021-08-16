using System.Threading;
using System.Threading.Tasks;

namespace Findx.EventBus
{
    /// <summary>
    /// 事件发送者
    /// </summary>
    public interface IEventDispatcher
    {
        void EnqueueToPublish(EventMediumMessage message);

        Task EnqueueToPublishAsync(EventMediumMessage message, CancellationToken cancellationToken = default);

        Task EnqueueToExecuteAsync(EventMediumMessage message, CancellationToken cancellationToken = default);
    }
}
