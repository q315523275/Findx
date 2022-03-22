using Findx.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    /// <summary>
    /// 消息通知者
    /// </summary>
    public class DefaultMessageNotifySender : IMessageNotifySender, IDisposable
    {
        private readonly ConcurrentDictionary<Type, object> _messageHandlers = new ConcurrentDictionary<Type, object>();

        private readonly IConfiguration _configuration;
        private readonly Channel<IMessageNotify> _channel;
        private readonly SemaphoreSlim _connectionLock;
        private readonly ILogger<DefaultMessageNotifySender> _logger;
        private readonly CancellationTokenSource _cancellationToken;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public DefaultMessageNotifySender(IConfiguration configuration, ILogger<DefaultMessageNotifySender> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var _queueCapacity = _configuration.GetValue<int>("Findx:MessageQueueCapacity");
            if (_queueCapacity > 0)
                _channel = Channel.CreateBounded<IMessageNotify>(new BoundedChannelOptions(_queueCapacity) { FullMode = BoundedChannelFullMode.Wait });
            else
                _channel = Channel.CreateUnbounded<IMessageNotify>();

            var maxTaskCount = _configuration.GetValue<int>("Findx:MessageHanderMaxTaskCount");
            maxTaskCount = maxTaskCount == 0 ? Environment.ProcessorCount + 1 : maxTaskCount;
            maxTaskCount = maxTaskCount <= 0 ? 1 : maxTaskCount;


            _connectionLock = new SemaphoreSlim(maxTaskCount);
            _cancellationToken = new CancellationTokenSource();

            StartConsuming(_cancellationToken.Token);
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            _cancellationToken?.Cancel();
            _messageHandlers?.Clear();
        }

        /// <summary>
        /// 推送消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessageNotify
        {
            Check.NotNull(message, nameof(message));

            await _channel.Writer.WriteAsync(message, cancellationToken);
        }

        /// <summary>
        /// 开始消费
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void StartConsuming(CancellationToken cancellationToken = default)
        {
            Task.Factory.StartNew(async () =>
            {
                while (await _channel.Reader.WaitToReadAsync())
                {
                    while (_channel.Reader.TryRead(out var message))
                    {
                        await _connectionLock.WaitAsync();
                        ProcessAsync(message, cancellationToken).ContinueWith(t => _connectionLock.Release());
                    }
                }
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task ProcessAsync(IMessageNotify message, CancellationToken cancellationToken = default)
        {
            try
            {
                var messageType = message.GetType();

                using var scope = ServiceLocator.ServiceProvider.CreateScope();

                var handler = (MessageNotifyHandlerWrapper)_messageHandlers.GetOrAdd(messageType, t => Activator.CreateInstance(typeof(MessageNotifyHandlerWrapperImpl<>).MakeGenericType(messageType)));
                await handler.Handle(message, scope.ServiceProvider, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "进程消息执行失败");
            }
        }
    }
}
