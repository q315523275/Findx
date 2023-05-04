using System.Threading;
using System.Threading.Tasks;

namespace Findx.EventBus
{
    /// <summary>
    ///     事件处理器
    /// </summary>
    public interface IEventHandler
    {
        /// <summary>
        ///     异步事件处理
        /// </summary>
        /// <param name="eventData">事件源数据</param>
        /// <param name="cancelToken">异步取消标识</param>
        /// <returns></returns>
        Task HandleAsync(IEventData eventData, CancellationToken cancelToken = default);
    }
}