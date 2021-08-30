using System.Threading;
using System.Threading.Tasks;

namespace Findx.EventBus
{
    /// <summary>
    /// 事件推送器
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// 事务
        /// </summary>
        AsyncLocal<IEventTransaction> Transaction { get; }

        /// <summary>
        /// 推送事件
        /// </summary>
        /// <param name="eventData"></param>
        void Publish(IntegrationEvent eventData);

        /// <summary>
        /// 推送事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task PublishAsync(IntegrationEvent eventData, CancellationToken cancellationToken = default);
    }
}
