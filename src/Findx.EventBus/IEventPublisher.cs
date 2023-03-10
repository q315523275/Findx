using System.Threading;
using System.Threading.Tasks;
using Findx.Threading;

namespace Findx.EventBus
{
    /// <summary>
    /// 事件发布器
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// 事件工作单元
        /// </summary>
        IValueAccessor<IEventUnitOfWork> UnitOfWork { get; }
        
        /// <summary>
        /// 异步发布事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task PublishAsync(IEventData eventData, CancellationToken cancellationToken = default);
    }
}