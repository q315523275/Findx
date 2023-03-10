using System.Threading;
using System.Threading.Tasks;

namespace Findx.EventBus
{

    /// <summary>
    /// 事件订阅执行器
    /// </summary>
    public interface IEventSubscribeExecutor
    {
        /// <summary>
        /// 事件执行
        /// </summary>
        /// <param name="eventData">事件内容</param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task ExecuteAsync(IEventData eventData, CancellationToken token = default);
    }
}