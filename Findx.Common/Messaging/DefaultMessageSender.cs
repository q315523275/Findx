using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    public class DefaultMessageSender : IMessageSender
    {
        private static readonly ConcurrentDictionary<Type, object> _messageHandlers = new ConcurrentDictionary<Type, object>();

        private readonly IServiceProvider _serviceProvider;

        public DefaultMessageSender(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TResponse> SendAsync<TResponse>(IMessageRequest<TResponse> message, CancellationToken cancellationToken = default)
        {
            Check.NotNull(message, nameof(message));


            var messageType = message.GetType();

            var handler = (MessageEventHandlerWrapper<TResponse>)_messageHandlers.GetOrAdd(messageType,
                t => Activator.CreateInstance(typeof(MessageEventHandlerWrapperImpl<,>).MakeGenericType(messageType, typeof(TResponse))));

            Check.NotNull(handler, nameof(handler));

            return handler.Handle(message, _serviceProvider, cancellationToken);
        }
    }
}
