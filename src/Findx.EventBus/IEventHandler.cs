using System.Threading.Tasks;

namespace Findx.EventBus
{
    /// <summary>
    /// 泛型事件处理器
    /// </summary>
    /// <typeparam name="TIntegrationEvent"></typeparam>
    public interface IEventHandler<in TIntegrationEvent> : IEventHandler
        where TIntegrationEvent : IntegrationEvent
    {
        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task HandleAsync(TIntegrationEvent @event);
    }

    /// <summary>
    /// 事件处理器
    /// </summary>
    public interface IEventHandler
    {
    }
}
