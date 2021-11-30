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
    public class DefaultMessageNotifySender : IMessageNotifySender, IDisposable
    {
        private static readonly ConcurrentDictionary<Type, object> _messageHandlers = new ConcurrentDictionary<Type, object>();

        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly Channel<IMessageNotify> _channel;
        private readonly SemaphoreSlim _connectionLock;
        private readonly ILogger<DefaultMessageNotifySender> _logger;
        private readonly CancellationTokenSource _cancellationToken;

        public DefaultMessageNotifySender(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<DefaultMessageNotifySender> logger)
        {
            _serviceProvider = serviceProvider;
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

        public void Dispose()
        {
            _cancellationToken?.Cancel();
        }

        public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessageNotify
        {
            Check.NotNull(message, nameof(message));

            await _channel.Writer.WriteAsync(message, cancellationToken);
        }

        public void StartConsuming(CancellationToken cancellationToken = default)
        {
            Task.Factory.StartNew(async () =>
            {
                while (await _channel.Reader.WaitToReadAsync())
                {
                    await _connectionLock.WaitAsync();

                    var message = await _channel.Reader.ReadAsync();

                    _ = Task.Run(async () => { await ProcessAsync(message); }, cancellationToken).ContinueWith(x => { _connectionLock.Release(); x.Dispose(); });
                }
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }


        protected async Task ProcessAsync(IMessageNotify message)
        {
            try
            {
                var messageType = message.GetType();

                using var scope = _serviceProvider.CreateScope();

                var handler = (MessageNotifyHandlerWrapper)_messageHandlers.GetOrAdd(messageType, t => ActivatorUtilities.CreateInstance(scope.ServiceProvider, typeof(MessageNotifyHandlerWrapperImpl<>).MakeGenericType(messageType)));
                await handler.Handle(message, scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "进程消息执行失败");
            }
        }
    }
}
