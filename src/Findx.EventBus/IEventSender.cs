using System.Threading;
using System.Threading.Tasks;

namespace Findx.EventBus
{
    /// <summary>
    /// 事件真实发送者
    /// </summary>
    public interface IEventSender
    {
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        void Send(TransportMessage message);

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SendAsync(TransportMessage message, CancellationToken cancellationToken = default);
    }
}
