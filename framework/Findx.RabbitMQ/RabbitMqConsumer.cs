using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Findx.ExceptionHandling;
using Findx.Threading;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Findx.RabbitMQ
{
    public class RabbitMqConsumer : IRabbitMqConsumer, IDisposable
    {
        public RabbitMqConsumer(IConnectionPool connectionPool, AsyncTimer timer, IExceptionNotifier exceptionNotifier,
            ILogger<RabbitMqConsumer> logger)
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

        protected AsyncTimer Timer { get; }

        public virtual void Dispose()
        {
            Timer.Stop();
            DisposeChannel();
        }

        public Func<IModel, BasicDeliverEventArgs, Task> FailedCallback { get; set; }

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

        public void Initialize([NotNull] ExchangeDeclareConfiguration exchange,
            [NotNull] QueueDeclareConfiguration queue, string connectionName = null)
        {
            Exchange = Check.NotNull(exchange, nameof(exchange));
            Queue = Check.NotNull(queue, nameof(queue));
            ConnectionName = connectionName;
            Timer.Start();
        }

        protected virtual async Task Timer_Elapsed(AsyncTimer timer)
        {
            if (Channel == null || Channel.IsOpen == false)
            {
                await TryCreateChannelAsync();
                await TrySendQueueBindCommandsAsync();
            }
        }

        protected virtual async Task HandleIncomingMessageAsync(object sender,
            BasicDeliverEventArgs basicDeliverEventArgs)
        {
            try
            {
                foreach (var callback in Callbacks) await callback(Channel, basicDeliverEventArgs);

                if (Queue.AutoAck)
                    Channel.BasicAck(basicDeliverEventArgs.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                try
                {
                    if (Queue.AutoAck)
                        Channel.BasicNack(basicDeliverEventArgs.DeliveryTag, false, true);

                    if (FailedCallback != null)
                        await FailedCallback(Channel, basicDeliverEventArgs);
                }
                catch
                {
                    // ignored
                }

                Logger.LogError(ex, "rabbitMqConsumer handleIncomingMessageAsync");
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
                    Exchange.ExchangeName,
                    Exchange.Type,
                    Exchange.Durable,
                    Exchange.AutoDelete,
                    Exchange.Arguments
                );

                Channel.QueueDeclare(
                    Queue.QueueName,
                    Queue.Durable,
                    Queue.Exclusive,
                    Queue.AutoDelete,
                    Queue.Arguments
                );

                // var consumer = new AsyncEventingBasicConsumer(Channel);
                var consumer = new EventingBasicConsumer(Channel);
                consumer.Received += async (_, basicDeliverEventArgs) =>
                {
                    await HandleIncomingMessageAsync(Channel, basicDeliverEventArgs);
                };

                Channel.BasicQos(0, (ushort)Queue.Qos, false);

                Channel.BasicConsume(Queue.QueueName, Queue.AutoAck, consumer);
            }
            catch (Exception ex)
            {
                if (ex is OperationInterruptedException operationInterruptedException &&
                    operationInterruptedException.ShutdownReason.ReplyCode == 406 &&
                    operationInterruptedException.Message.Contains("arg 'x-dead-letter-exchange'"))
                {
                    Logger.LogWarning(ex, "rabbitMqConsumer tryCreateChannelAsync");
                    await ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));
                }

                Logger.LogWarning(ex, "rabbitMqConsumer tryCreateChannelAsync");
                await ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));
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
                ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));
            }
        }

        protected virtual async Task TrySendQueueBindCommandsAsync()
        {
            try
            {
                while (!QueueBindCommands.IsEmpty)
                {
                    if (Channel == null || Channel.IsClosed) return;

                    lock (ChannelSendSyncLock)
                    {
                        if (QueueBindCommands.TryPeek(out var command))
                        {
                            switch (command.Type)
                            {
                                case QueueBindType.Bind:
                                    Channel.QueueBind(
                                        Queue.QueueName,
                                        Exchange.ExchangeName,
                                        command.RoutingKey
                                    );
                                    break;
                                case QueueBindType.Unbind:
                                    Channel.QueueUnbind(
                                        Queue.QueueName,
                                        Exchange.ExchangeName,
                                        command.RoutingKey
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

        protected class QueueBindCommand
        {
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
}