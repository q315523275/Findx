using Findx.EventBus.Events;

namespace Findx.EventBus.Abstractions
{
    /// <summary>
    /// 事件订阅器
    /// </summary>
    public interface IEventSubscriber
    {
        /// <summary>
        /// 事件订阅
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IEventHandler<T>;

        /// <summary>
        /// 动态事件订阅
        /// </summary>
        /// <typeparam name="TH"></typeparam>
        /// <param name="eventName"></param>
        void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicEventHandler;

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        void Unsubscribe<T, TH>()
            where TH : IEventHandler<T>
            where T : IntegrationEvent;

        /// <summary>
        /// 取消动态订阅事件
        /// </summary>
        /// <typeparam name="TH"></typeparam>
        /// <param name="eventName"></param>
        void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicEventHandler;

        /// <summary>
        /// 开始事件消费处理
        /// </summary>
        void StartConsuming();
    }
}
