using Findx.DependencyInjection;
using Findx.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    /// <summary>
    /// 消息通知者
    /// </summary>
    public class ApplicationEventPublisher : IApplicationEventPublisher, IDisposable
    {
        private readonly IDictionary<Type, object> _eventHandlers = new Dictionary<Type, object>();

        private readonly Channel<IApplicationEvent> _channel;
        private readonly ILogger<ApplicationEventPublisher> _logger;
        private readonly CancellationTokenSource _cts;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public ApplicationEventPublisher(IConfiguration configuration, ILogger<ApplicationEventPublisher> logger)
        {
            _logger = logger;

            var queueCapacity = configuration.GetValue<int>("Findx:MessageQueueCapacity");
            _channel = queueCapacity > 0 ? Channel.CreateBounded<IApplicationEvent>(new BoundedChannelOptions(queueCapacity) { FullMode = BoundedChannelFullMode.Wait }) : Channel.CreateUnbounded<IApplicationEvent>();

            var consumerThreadCount = configuration.GetValue<int>("Findx:MessageHandlerMaxTaskCount");
            consumerThreadCount = consumerThreadCount == 0 ? Environment.ProcessorCount + 1 : consumerThreadCount;
            consumerThreadCount = consumerThreadCount <= 0 ? 1 : consumerThreadCount;

            _cts = new CancellationTokenSource();

            // StartConsuming(_cancellationToken.Token);
            Task.WhenAll(Enumerable.Range(0, consumerThreadCount)
                   .Select(_ => Task.Factory.StartNew(() => Processing(_channel, _cts.Token), _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default)));
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            _cts?.Cancel();
            _eventHandlers?.Clear();
        }

        /// <summary>
        /// 推送消息
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="applicationEvent"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task PublishAsync<TEvent>(TEvent applicationEvent, CancellationToken cancellationToken = default) where TEvent : IApplicationEvent
        {
            Check.NotNull(applicationEvent, nameof(applicationEvent));

            await _channel.Writer.WriteAsync(applicationEvent, cancellationToken);
        }

        /// <summary>
        /// 消费执行方法
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task Processing(Channel<IApplicationEvent> channel, CancellationToken cancellationToken)
        {
            try
            {
                while (await channel.Reader.WaitToReadAsync(cancellationToken))
                {
                    while (channel.Reader.TryRead(out var message))
                    {
                        try
                        {
                            var messageType = message.GetType();

                            using (var scope = ServiceLocator.ServiceProvider.CreateScope())
                            {
                                var handler = (ApplicationEventHandlerWrapper)_eventHandlers.GetOrAdd(messageType, t => Activator.CreateInstance(typeof(ApplicationEventHandlerWrapperImpl<>).MakeGenericType(messageType)));
                                await handler.Handle(message, scope.ServiceProvider, cancellationToken);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            //expected
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, $"An exception occurred when invoke notify subscriber");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // expected
            }
        }
    }
}
