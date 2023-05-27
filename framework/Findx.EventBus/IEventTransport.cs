using System.Threading;
using System.Threading.Tasks;

namespace Findx.EventBus
{
    /// <summary>
    ///     事件传输器
    /// </summary>
    public interface IEventTransport
    {
        /// <summary>
        ///     异步发送事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SendAsync(IEventData eventData, CancellationToken cancellationToken = default);
    }
}