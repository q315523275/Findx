using Findx.Extensions;
using Findx.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Findx.EventBus
{
    /// <summary>
    /// 事件订阅基类
    /// </summary>
    public abstract class EventSubscriberBase : IEventSubscriber
    {
        /// <summary>
        /// 订阅管理器
        /// </summary>
        protected IEventSubscribeManager SubscribeManager { get; }

        private readonly ILogger<EventSubscriberBase> _logger;
        private readonly IEventStore _storage;
        private readonly IEventDispatcher _dispatcher;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="subsManager"></param>
        /// <param name="storage"></param>
        /// <param name="logger"></param>
        /// <param name="dispatcher"></param>
        protected EventSubscriberBase(IEventSubscribeManager subsManager, IEventStore storage, ILogger<EventSubscriberBase> logger, IEventDispatcher dispatcher)
        {
            SubscribeManager = subsManager;
            SubscribeManager.OnEventRemoved += SubsManager_OnEventRemoved;

            _storage = storage;
            _logger = logger;
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        public void Subscribe<T, TH>() where T : IntegrationEvent where TH : IEventHandler<T>
        {
            var eventName = EventNameAttribute.GetNameOrDefault(typeof(T));

            var eventHandlerParameter = EventConsumerAttribute.GetHandlerParameterOrDefault(typeof(TH));

            DoInternalSubscribe(eventName, eventHandlerParameter.Item1, eventHandlerParameter.Item2);

            SubscribeManager.AddSubscribe<T, TH>();
        }

        /// <summary>
        /// 订阅动态事件
        /// </summary>
        /// <typeparam name="TH"></typeparam>
        public void SubscribeDynamic<TH>() where TH : IDynamicEventHandler
        {
            var handlerAttribute = typeof(TH).GetAttribute<EventNameAttribute>();
            Check.NotNull(handlerAttribute, nameof(handlerAttribute));

            var eventName = handlerAttribute.Name;

            var eventHandlerParameter = EventConsumerAttribute.GetHandlerParameterOrDefault(typeof(TH));

            DoInternalSubscribe(eventName, eventHandlerParameter.Item1, eventHandlerParameter.Item2);

            SubscribeManager.AddDynamicSubscribe<TH>(eventName);
        }

        /// <summary>
        /// 订阅动态事件
        /// </summary>
        /// <typeparam name="TH"></typeparam>
        /// <param name="eventName"></param>
        public void SubscribeDynamic<TH>(string eventName) where TH : IDynamicEventHandler
        {
            var eventHandlerParameter = EventConsumerAttribute.GetHandlerParameterOrDefault(typeof(TH));

            DoInternalSubscribe(eventName, eventHandlerParameter.Item1, eventHandlerParameter.Item2);

            SubscribeManager.AddDynamicSubscribe<TH>(eventName);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        public void Unsubscribe<T, TH>() where T : IntegrationEvent where TH : IEventHandler<T>
        {
            SubscribeManager.RemoveSubscribe<T, TH>();
        }

        /// <summary>
        /// 取消动态订阅
        /// </summary>
        /// <typeparam name="TH"></typeparam>
        /// <param name="eventName"></param>
        public void UnsubscribeDynamic<TH>(string eventName) where TH : IDynamicEventHandler
        {
            SubscribeManager.RemoveDynamicSubscribe<TH>(eventName);
        }

        /// <summary>
        /// 事件移除后执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventName"></param>
        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            OnEventRemoved(eventName);
        }

        /// <summary>
        /// 内部订阅
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handlerName"></param>
        /// <param name="prefetchCount"></param>
        protected abstract void DoInternalSubscribe(string eventName, string handlerName, int prefetchCount);

        /// <summary>
        /// 事件被移除
        /// </summary>
        /// <param name="eventName"></param>
        protected abstract void OnEventRemoved(string eventName);

        /// <summary>
        /// 消息确认
        /// </summary>
        /// <param name="msgobj"></param>
        protected abstract void Commit(object msgobj);

        /// <summary>
        /// 消息回退
        /// </summary>
        protected abstract void Reject(object msgobj);

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected async Task ProcessAsync(ProcessingContext context)
        {
            try
            {
                Check.NotNull(context, nameof(context));

                var transportMessage = context.TransportMessage;

                if (SubscribeManager.HasSubscribeForEvent(transportMessage.GetEventName()))
                {
                    var mediumMessage = _storage.SaveReceivedEvent(transportMessage.GetId(), transportMessage.GetEventName(), System.Text.Encoding.UTF8.GetString(transportMessage.Body));

                    Commit(context.EventObject);

                    await _dispatcher.EnqueueToExecuteAsync(mediumMessage);
                }
                else
                {
                    _storage.SaveReceivedExceptionEvent(transportMessage.GetId(), transportMessage.GetEventName(), System.Text.Encoding.UTF8.GetString(transportMessage.Body));

                    Commit(context.EventObject);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"事件总线接收处理异常, EventName:'{context.TransportMessage.GetEventName()}' Message:'{System.Text.Encoding.UTF8.GetString(context.TransportMessage.Body)}'.");

                Reject(context.EventObject);
            }
        }

        /// <summary>
        /// 开始消费
        /// </summary>
        public abstract void StartConsuming();
    }
}
