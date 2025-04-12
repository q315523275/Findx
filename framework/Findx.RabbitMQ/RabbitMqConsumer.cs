using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Findx.ExceptionHandling;
using Findx.Locks;
using Findx.Threading;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Findx.RabbitMQ;

/// <summary>
///     MQ消费者
/// </summary>
public class RabbitMqConsumer : IRabbitMqConsumer, IDisposable
{
    public RabbitMqConsumer(IConnectionPool connectionPool, AsyncTimer timer, IExceptionNotifier exceptionNotifier, ILogger<RabbitMqConsumer> logger)
    {
        ConnectionPool = connectionPool;
        Timer = timer;
        ExceptionNotifier = exceptionNotifier;
        Logger = logger;

        QueueBindCommands = new ConcurrentQueue<QueueBindCommand>();
        Callbacks = [];

        Timer.Period = 5000; //5 sec.
        Timer.Elapsed = Timer_Elapsed;
        Timer.RunOnStart = true;
    }

    public ILogger<RabbitMqConsumer> Logger { get; set; }

    protected IConnectionPool ConnectionPool { get; }

    protected IExceptionNotifier ExceptionNotifier { get; }

    protected IChannel Channel { get; private set; }

    protected ExchangeDeclareConfiguration Exchange { get; private set; }

    protected QueueDeclareConfiguration Queue { get; private set; }

    protected string ConnectionName { get; private set; }

    protected ConcurrentBag<Func<IChannel, BasicDeliverEventArgs, Task>> Callbacks { get; }

    protected ConcurrentQueue<QueueBindCommand> QueueBindCommands { get; }

    protected AsyncLock ChannelSendSyncLock { get; } = new();

    protected AsyncTimer Timer { get; }

    public virtual void Dispose()
    {
        Timer.Stop();
        DisposeChannel();
    }

    public Func<IChannel, BasicDeliverEventArgs, CancellationToken, Task> FailedCallback { get; set; }

    public void Initialize([NotNull] ExchangeDeclareConfiguration exchange, [NotNull] QueueDeclareConfiguration queue, string connectionName = null)
    {
        Exchange = Check.NotNull(exchange, nameof(exchange));
        Queue = Check.NotNull(queue, nameof(queue));
        ConnectionName = connectionName;
        Timer.Start();
    }
    
    public virtual async Task BindAsync(string routingKey, CancellationToken cancellationToken = default)
    {
        QueueBindCommands.Enqueue(new QueueBindCommand(QueueBindType.Bind, routingKey));
        await TrySendQueueBindCommandsAsync(cancellationToken);
    }

    public virtual async Task UnbindAsync(string routingKey, CancellationToken cancellationToken = default)
    {
        QueueBindCommands.Enqueue(new QueueBindCommand(QueueBindType.Unbind, routingKey));
        await TrySendQueueBindCommandsAsync(cancellationToken);
    }
    
    protected virtual async Task TrySendQueueBindCommandsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            while (!QueueBindCommands.IsEmpty)
            {
                if (Channel == null || Channel.IsClosed) return;

                using (await ChannelSendSyncLock.LockAsync(cancellationToken))
                {
                    if (QueueBindCommands.TryPeek(out var command))
                    {
                        switch (command.Type)
                        {
                            case QueueBindType.Bind:
                                await Channel.QueueBindAsync(
                                    Queue.QueueName,
                                    Exchange.ExchangeName,
                                    command.RoutingKey, cancellationToken: cancellationToken);
                                break;
                            case QueueBindType.Unbind:
                                await Channel.QueueUnbindAsync(
                                    Queue.QueueName,
                                    Exchange.ExchangeName,
                                    command.RoutingKey, cancellationToken: cancellationToken);
                                break;
                            default:
                                throw new Exception($"Unknown {nameof(QueueBindType)}: {command.Type}");
                        }

                        QueueBindCommands.TryDequeue(out command);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "");
            await ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex), cancellationToken);
        }
    }
    
    public virtual void OnMessageReceived(Func<IChannel, BasicDeliverEventArgs, Task> callback)
    {
        Callbacks.Add(callback);
    }

    protected virtual async Task Timer_Elapsed(AsyncTimer timer)
    {
        if (Channel == null || Channel.IsOpen == false)
        {
            await TryCreateChannelAsync();
            await TrySendQueueBindCommandsAsync();
        }
    }

    protected virtual async Task TryCreateChannelAsync(CancellationToken cancellationToken = default)
    {
        await DisposeChannelAsync();

        try
        {
            Channel = await (await ConnectionPool.GetAsync(ConnectionName, cancellationToken)).CreateChannelAsync(cancellationToken: cancellationToken);

            await Channel.ExchangeDeclareAsync(
                Exchange.ExchangeName,
                Exchange.Type,
                Exchange.Durable,
                Exchange.AutoDelete,
                Exchange.Arguments, cancellationToken: cancellationToken);

            await Channel.QueueDeclareAsync(
                Queue.QueueName,
                Queue.Durable,
                Queue.Exclusive,
                Queue.AutoDelete,
                Queue.Arguments, cancellationToken: cancellationToken);

            // var consumer = new AsyncEventingBasicConsumer(Channel);
            // EventingBasicConsumer开启多线程
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.ReceivedAsync += (_, basicDeliverEventArgs) =>
            {
                HandleIncomingMessageAsync(Channel, basicDeliverEventArgs, cancellationToken);
                return Task.CompletedTask;
            };

            await Channel.BasicQosAsync(0, (ushort)Queue.Qos, false, cancellationToken);

            // 默认必须触发消息ack
            await Channel.BasicConsumeAsync(Queue.QueueName, false, consumer, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            if (ex is OperationInterruptedException { ShutdownReason.ReplyCode: 406 } operationInterruptedException &&
                operationInterruptedException.Message.Contains("arg 'x-dead-letter-exchange'"))
            {
                Logger.LogWarning(ex, "RabbitMQConsumer tryCreateChannelAsync");
                await ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex), cancellationToken);
            }

            Logger.LogWarning(ex, "RabbitMQConsumer tryCreateChannelAsync");
            await ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex), cancellationToken);
        }
    }
    
    protected virtual async Task HandleIncomingMessageAsync(object sender, BasicDeliverEventArgs basicDeliverEventArgs, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var callback in Callbacks)
            {
                await callback(Channel, basicDeliverEventArgs);
            }

            if (Queue.AutoAck && Channel != null)
            {
                await Channel.BasicAckAsync(basicDeliverEventArgs.DeliveryTag, false, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            try
            {
                if (Queue.AutoAck && Channel != null)
                {
                    await Channel.BasicAckAsync(basicDeliverEventArgs.DeliveryTag, false, cancellationToken);
                }

                if (FailedCallback != null)
                {
                    await FailedCallback(Channel, basicDeliverEventArgs, cancellationToken);
                }
            }
            catch
            {
                // ignored
            }

            Logger.LogError(ex, "RabbitMQConsumer handleIncomingMessageAsync");
            await ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex), cancellationToken);
        }
    }
    
    protected virtual async Task DisposeChannelAsync()
    {
        if (Channel == null) return;

        try
        {
            Channel.Dispose();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "");
            await ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));
        }
    }

    protected virtual void DisposeChannel()
    {
        if (Channel == null) return;

        try
        {
            Channel.Dispose();
            Channel = null;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "");
            ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex)).ConfigureAwait(false);
        }
    }
    
    protected class QueueBindCommand
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="routingKey"></param>
        public QueueBindCommand(QueueBindType type, string routingKey)
        {
            Type = type;
            RoutingKey = routingKey;
        }

        public QueueBindType Type { get; }

        public string RoutingKey { get; }
    }

    protected enum QueueBindType
    {
        Bind,
        Unbind
    }
}