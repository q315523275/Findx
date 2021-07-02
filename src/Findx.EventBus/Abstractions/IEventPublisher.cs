using Findx.EventBus.Events;
using System.Threading.Tasks;

namespace Findx.EventBus.Abstractions
{
    /// <summary>
    /// 事件推送器
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// 推送事件消息
        /// </summary>
        /// <param name="event"></param>
        void Publish(IntegrationEvent @event);
        /// <summary>
        /// 推送事件消息
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task PublishAsync(IntegrationEvent @event);
    }
}
