using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Findx.EventBus
{
    /// <summary>
    ///     事件总线基类
    /// </summary>
    public abstract class EventBusBase : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        protected EventBusBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            EventManager = serviceProvider.GetRequiredService<IEventManager>();
            Logger = serviceProvider.GetLogger(GetType());
        }

        /// <summary>
        ///     获取 事件管理器
        /// </summary>
        protected IEventManager EventManager { get; }

        /// <summary>
        ///     获取 日志对象
        /// </summary>
        protected ILogger Logger { get; }

        #region Implementation of IEventSubscriber

        /// <summary>
        ///     事件工作单元
        /// </summary>
        public IValueAccessor<IEventUnitOfWork> UnitOfWork { get; } = new ValueAccessor<IEventUnitOfWork>();

        /// <summary>
        ///     异步发布事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task PublishAsync(IEventData eventData, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Implementation of IEventSubscriber

        /// <summary>
        ///     订阅指定事件和事件处理器
        /// </summary>
        /// <typeparam name="TEventData"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        /// <exception cref="NotImplementedException"></exception>
        public void Subscribe<TEventData, TEventHandler>() where TEventData : IEventData
            where TEventHandler : IEventHandler, new()
        {
            EventManager.Add<TEventData, TEventHandler>();
        }

        /// <summary>
        ///     移除指定事件和事件处理器
        /// </summary>
        /// <typeparam name="TEventData"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        /// <exception cref="NotImplementedException"></exception>
        public void Unsubscribe<TEventData, TEventHandler>() where TEventData : IEventData
            where TEventHandler : IEventHandler, new()
        {
            EventManager.Remove<TEventData, TEventHandler>();
        }

        #endregion
    }
}