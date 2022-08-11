using System.Threading;
using System.Threading.Tasks;

namespace Findx.EventBus
{
    /// <summary>
    /// 事件执行器
    /// </summary>
    public interface IEventExecutor
    {
        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        Task ExecuteAsync(EventMediumMessage message, CancellationToken cancellationToken = default);
    }
}
