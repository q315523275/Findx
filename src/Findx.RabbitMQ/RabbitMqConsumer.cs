using Findx.ExceptionHandling;
using Findx.Threading;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Findx.RabbitMQ
{
    public class RabbitMqConsumer : IRabbitMqConsumer, IDisposable
    {
        public Func<IModel, BasicDeliverEventArgs, Task> FailedCallback { get; set; }

        public ILogger<RabbitMqConsumer> Logger { get; set; }

        protected IConnectionPool ConnectionPool { get; }

        protected IExceptionNotifier ExceptionNotifier { get; }

        protected IModel Channel { get; private set; }

        protected ExchangeDeclareConfiguration Exchange { get; private set; }

        protected QueueDeclareConfiguration Queue { get; private set; }

        protected string ConnectionName { get; private set; }

        protected ConcurrentBag<Func<IModel, BasicDeliverEventArgs, Task>> Callbacks { get; }

        protected ConcurrentQueue<QueueBindCommand> QueueBindCommands { get; }

        protected object ChannelSendSyncLock { get; } = new object();

        protected FindxAsyncTimer Timer { get; }

        public RabbitMqConsumer(IConnectionPool connectionPool, FindxAsyncTimer timer, IExceptionNotifier exceptionNotifier, ILogger<RabbitMqConsumer> logger)
        {
            ConnectionPool = connectionPool;
            Timer = timer;
            ExceptionNotifier = exceptionNotifier;
            Logger = logger;

            QueueBindCommands = new ConcurrentQueue<QueueBindCommand>();
            Callbacks = new ConcurrentBag<Func<IModel, BasicDeliverEventArgs, Task>>();

            Timer.Period = 5000; //5 sec.
            Timer.Elapsed = Timer_Elapsed;
            Timer.RunOnStart = true;
        }

        public void Initialize([NotNull] ExchangeDeclareConfiguration exchange, [NotNull] QueueDeclareConfiguration queue, string connectionName = null)
        {
            Exchange = Check.NotNull(exchange, nameof(exchange));
            Queue = Check.NotNull(queue, nameof(queue));
            ConnectionName = connectionName;
            Timer.Start();
        }

        protected virtual async Task Timer_Elapsed(FindxAsyncTimer timer)
        {
            if (Channel == null || Channel.IsOpen == false)
            {
                await TryCreateChannelAsync();
                await TrySendQueueBindCommandsAsync();
            }
        }

        public virtual async Task BindAsync(string routingKey)
        {
            QueueBindCommands.Enqueue(new QueueBindCommand(QueueBindType.Bind, routingKey));
            await TrySendQueueBindCommandsAsync();
        }

        public virtual async Task UnbindAsync(string routingKey)
        {
            QueueBindCommands.Enqueue(new QueueBindCommand(QueueBindType.Unbind, routingKey));
            await TrySendQueueBindCommandsAsync();
        }

        public virtual void OnMessageReceived(Func<IModel, BasicDeliverEventArgs, Task> callback)
        {
            Callbacks.Add(callback);
        }

        protected virtual async Task HandleIncomingMessageAsync(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            try
            {
                foreach (var callback in Callbacks)
                {
                    await callback(Channel, basicDeliverEventArgs);
                }
            
                if (!Queue.AutoAck)
                    Channel.BasicAck(basicDeliverEventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                try
                {
                    Channel.BasicNack(basicDeliverEventArgs.DeliveryTag, multiple: false, requeue: true);

                    if (FailedCallback != null)
                        await FailedCallback(Channel, basicDeliverEventArgs);
                }
                catch
                {
                    // ignored
                }

                Logger.LogError(ex, ex.Message);
                await ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));
            }
        }

        protected virtual async Task TryCreateChannelAsync()
        {
            await DisposeChannelAsync();

            try
            {
                Channel = ConnectionPool.Get(ConnectionName).CreateModel();

                Channel.ExchangeDeclare(
                    exchange: Exchange.ExchangeName,
                    type: Exchange.Type,
                    durable: Exchange.Durable,
                    autoDelete: Exchange.AutoDelete,
                    arguments: Exchange.Arguments
                );

                Channel.QueueDeclare(
                    queue: Queue.QueueName,
                    durable: Queue.Durable,
                    exclusive: Queue.Exclusive,
                    autoDelete: Queue.AutoDelete,
                    arguments: Queue.Arguments
                );

                // var consumer = new AsyncEventingBasicConsumer(Channel);
                var consumer = new EventingBasicConsumer(Channel);
                consumer.Received += async (model, basicDeliverEventArgs) =>
                {
                    await HandleIncomingMessageAsync(Channel, basicDeliverEventArgs);
                };

                Channel.BasicQos(0, (ushort)Queue.Qos, false);

                Channel.BasicConsume(queue: Queue.QueueName, autoAck: Queue.AutoAck, consumer: consumer);

            }
            catch (Exception ex)
            {
                if (ex is OperationInterruptedException operationInterruptedException &&
                    operationInterruptedException.ShutdownReason.ReplyCode == 406 &&
                    operationInterruptedException.Message.Contains("arg 'x-dead-letter-exchange'"))
                {
                    Logger.LogWarning(ex, ex.Message);
                    await ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));
                }

                Logger.LogWarning(ex, ex.Message);
                await ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));
            }
        }

        protected virtual async Task DisposeChannelAsync()
        {
            if (Channel == null)
            {
                return;
            }

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
            if (Channel == null)
            {
                return;
            }

            try
            {
                Channel.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "");
                ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));
            }
        }

        protected virtual async Task TrySendQueueBindCommandsAsync()
        {
            try
            {
                while (!QueueBindCommands.IsEmpty)
                {
                    if (Channel == null || Channel.IsClosed)
                    {
                        return;
                    }

                    lock (ChannelSendSyncLock)
                    {
                        if (QueueBindCommands.TryPeek(out var command))
                        {
                            switch (command.Type)
                            {
                                case QueueBindType.Bind:
                                    Channel.QueueBind(
                                        queue: Queue.QueueName,
                                        exchange: Exchange.ExchangeName,
                                        routingKey: command.RoutingKey
                                    );
                                    break;
                                case QueueBindType.Unbind:
                                    Channel.QueueUnbind(
                                        queue: Queue.QueueName,
                                        exchange: Exchange.ExchangeName,
                                        routingKey: command.RoutingKey
                                    );
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
                await ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));
            }
        }

        public virtual void Dispose()
        {
            Timer.Stop();
            DisposeChannel();
        }

        protected class QueueBindCommand
        {
            public QueueBindType Type { get; }

            public string RoutingKey { get; }

            public QueueBindCommand(QueueBindType type, string routingKey)
            {
                Type = type;
                RoutingKey = routingKey;
            }
        }

        protected enum QueueBindType
        {
            Bind,
            Unbind
        }
    }
}
