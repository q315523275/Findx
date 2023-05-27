namespace Findx.EventBus
{
    /// <summary>
    ///     事件订阅器
    /// </summary>
    public interface IEventSubscriber
    {
        /// <summary>
        ///     订阅指定事件与事件处理
        /// </summary>
        /// <typeparam name="TEventData">事件数据</typeparam>
        /// <typeparam name="TEventHandler">事件处理器</typeparam>
        void Subscribe<TEventData, TEventHandler>() where TEventData : IEventData
            where TEventHandler : IEventHandler, new();

        /// <summary>
        ///     取消订阅指定事件数据的事件处理委托
        /// </summary>
        /// <typeparam name="TEventData">事件数据类型</typeparam>
        /// <typeparam name="TEventHandler">事件处理器</typeparam>
        void Unsubscribe<TEventData, TEventHandler>() where TEventData : IEventData
            where TEventHandler : IEventHandler, new();
    }
}