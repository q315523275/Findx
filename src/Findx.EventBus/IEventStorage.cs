using System.Threading;
using System.Threading.Tasks;
using Findx.Data;

namespace Findx.EventBus
{
    /// <summary>
    /// 事件存储器
    /// </summary>
    public interface IEventStorage
    {
        /// <summary>
        /// 异步保存事件内容
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task SavePublishedEventAsync(IEventData eventData, IEventUnitOfWork? unitOfWork = null, CancellationToken token = default);
        
        /// <summary>
        /// 异步保存接收执行异常的事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task SaveReceivedExceptionEventAsync(IEventData eventData, IEventUnitOfWork? unitOfWork = null, CancellationToken token = default);
    }
}