using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    public class DefaultMessageNotifySender : IMessageNotifySender
    {
        private static readonly ConcurrentDictionary<Type, Type> _messageHandlers = new ConcurrentDictionary<Type, Type>();

        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly Channel<IMessageNotify> _channel;
        private readonly SemaphoreSlim _connectionLock;
        private readonly ILogger<DefaultMessageNotifySender> _logger;

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
            maxTaskCount = maxTaskCount == 0 ? Environment.ProcessorCount - 1 : maxTaskCount;
            maxTaskCount = maxTaskCount <= 0 ? 1 : maxTaskCount;
            _connectionLock = new SemaphoreSlim(maxTaskCount);

            StartConsuming();
        }

        public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessageNotify
        {
            Check.NotNull(message, nameof(message));

            await _channel.Writer.WriteAsync(message);
        }

        public void StartConsuming()
        {
            var taskFactory = new TaskFactory(TaskScheduler.Current);

            taskFactory.StartNew(async () =>
            {
                while (await _channel.Reader.WaitToReadAsync())
                {
                    await _connectionLock.WaitAsync();

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            if (_channel.Reader.TryRead(out var message))
                            {
                                var messageType = message.GetType();
                                var messageHanderType = _messageHandlers.GetOrAdd(messageType, t => typeof(IMessageNotifyHandler<>).MakeGenericType(messageType));

                                using (var scope = _serviceProvider.CreateScope())
                                {
                                    var serviceProvider = scope.ServiceProvider;

                                    var handlers = serviceProvider.GetServices(messageHanderType);
                                    if (handlers == null || handlers.Count() == 0)
                                        return;

                                    foreach (var handler in handlers)
                                    {
                                        await Task.Yield();
                                        await (Task)messageHanderType.GetMethod("Handle").Invoke(handler, new object[] { message, CancellationToken.None });
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Message notification execution exception");
                        }
                        finally
                        {
                            _connectionLock.Release();
                        }
                    });
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
