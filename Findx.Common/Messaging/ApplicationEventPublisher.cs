using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    public class ApplicationEventPublisher : IApplicationEventPublisher
    {
        private readonly IServiceProvider _serviceProvider;

        private static readonly ConcurrentDictionary<Type, object> _messageHandlers = new ConcurrentDictionary<Type, object>();
        private static readonly ConcurrentDictionary<Type, AsyncApplicationEventHandlerWrapper> _asyncMessageHandlers = new ConcurrentDictionary<Type, AsyncApplicationEventHandlerWrapper>();
        public ApplicationEventPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IAsyncApplicationEvent
        {
            Check.NotNull(message, nameof(message));

            var notificationType = message.GetType();
            var handler = _asyncMessageHandlers.GetOrAdd(notificationType,
                t => (AsyncApplicationEventHandlerWrapper)Activator.CreateInstance(typeof(AsyncApplicationEventHandlerWrapperImpl<>).MakeGenericType(notificationType)));

            return handler.Handle(message, cancellationToken, _serviceProvider, PublishCore);

        }

        protected virtual async Task PublishCore(IEnumerable<Func<IAsyncApplicationEvent, CancellationToken, Task>> allHandlers, IAsyncApplicationEvent applicationEvent, CancellationToken cancellationToken)
        {
            foreach (var handler in allHandlers)
            {
                await handler(applicationEvent, cancellationToken).ConfigureAwait(false);
            }
        }

        public Task<TResponse> SendAsync<TResponse>(IApplicationEvent<TResponse> message, CancellationToken cancellationToken = default)
        {
            Check.NotNull(message, nameof(message));

            #region 反射
            //var messageType = message.GetType();

            //var concreteType = typeof(IMessageHandler<,>).MakeGenericType(messageType, typeof(TResponse));

            //var handler = _serviceProvider.GetRequiredService(concreteType);

            //Check.NotNull(handler, nameof(handler));

            //return (Task<TResponse>)concreteType.GetMethod("Handle").Invoke(handler, new object[] { message, cancellationToken });
            #endregion

            var messageType = message.GetType();

            var handler = (ApplicationEventHandlerWrapper<TResponse>)_messageHandlers.GetOrAdd(messageType,
                t => Activator.CreateInstance(typeof(ApplicationEventHandlerWrapperImpl<,>).MakeGenericType(messageType, typeof(TResponse))));

            Check.NotNull(handler, nameof(handler));

            return handler.Handle(message, _serviceProvider, cancellationToken);
        }
    }
}
