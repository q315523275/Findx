using System.Threading.Tasks;
using Findx.Extensions;

namespace Findx.Messaging
{
    /// <summary>
    /// 进程消息发送者
    /// </summary>
    public class MessageDispatcher : IMessageDispatcher
    {
        /// <summary>
        /// 消息订阅处理器集合
        /// </summary>
        private static readonly IDictionary<Type, object> MessageHandlers = new ConcurrentDictionary<Type, object>();

        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public MessageDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 异步发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        public Task<TResponse> SendAsync<TResponse>(IMessageRequest<TResponse> message, CancellationToken cancellationToken = default)
        {
            Check.NotNull(message, nameof(message));

            var messageType = message.GetType();

            var handler = (MessageHandlerWrapper<TResponse>)MessageHandlers.GetOrAdd(messageType,
                         t => Activator.CreateInstance(typeof(MessageHandlerWrapperImpl<,>).MakeGenericType(messageType, typeof(TResponse))));

            Check.NotNull(handler, nameof(handler));

            return handler.Handle(message, _serviceProvider, cancellationToken);
        }
    }
}
