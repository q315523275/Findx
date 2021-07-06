using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.RabbitMQ
{
    public class RabbitMQConsumer : IRabbitMQConsumer, IDisposable
    {
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IConnectionPool _connectionPool;
        private readonly SemaphoreSlim _connectionLock;
        private readonly ConcurrentBag<Func<IModel, BasicDeliverEventArgs, Task>> _callbacks;
        private readonly ConcurrentQueue<QueueBindCommand> _queueBindCommands;

        private ExchangeDeclareConfiguration _exchange;
        private QueueDeclareConfiguration _queue;

        private IModel _channel;

        public RabbitMQConsumer(ILogger<RabbitMQConsumer> logger, IConnectionPool connectionPool, ExchangeDeclareConfiguration exchange, QueueDeclareConfiguration queue)
        {
            Check.NotNull(exchange, nameof(exchange));
            Check.NotNull(queue, nameof(queue));
            Check.NotNull(connectionPool, nameof(connectionPool));

            _logger = logger;
            _connectionPool = connectionPool;
            _exchange = exchange;
            _queue = queue;

            _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
            _callbacks = new ConcurrentBag<Func<IModel, BasicDeliverEventArgs, Task>>();
            _queueBindCommands = new ConcurrentQueue<QueueBindCommand>();
        }

        private bool IsConnected { get { return _channel != null && _channel.IsOpen; } }

        private bool TryConnect()
        {
            _connectionLock.Wait();
            try
            {
                if (!IsConnected)
                {
                    _channel = _connectionPool.Acquire().CreateModel();
                }
            }
            finally
            {
                _connectionLock.Release();
            }
            if (IsConnected)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Bind(string routingKey)
        {
            if (IsConnected)
            {
                _channel.QueueBind(queue: _queue.QueueName, exchange: _exchange.ExchangeName, routingKey: routingKey);
            }
            else
            {
                _queueBindCommands.Enqueue(new QueueBindCommand(QueueBindType.Bind, routingKey));
            }
        }

        public void Unbind(string routingKey)
        {
            if (IsConnected)
            {
                _channel.QueueUnbind(queue: _queue.QueueName, exchange: _exchange.ExchangeName, routingKey: routingKey);
            }
            else
            {
                _queueBindCommands.Enqueue(new QueueBindCommand(QueueBindType.Unbind, routingKey));
            }
        }

        public void StartConsuming()
        {
            if (!IsConnected)
            {
                TryConnect();
            }

            _channel.ExchangeDeclare(exchange: _exchange.ExchangeName, type: _exchange.Type, durable: _exchange.Durable, autoDelete: _exchange.AutoDelete, arguments: _exchange.Arguments);

            _channel.QueueDeclare(queue: _queue.QueueName, durable: _queue.Durable, exclusive: _queue.Exclusive, autoDelete: _queue.AutoDelete, arguments: _queue.Arguments);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, basicDeliverEventArgs) =>
            {
                await HandleIncomingMessage(_channel, basicDeliverEventArgs);
            };

            _channel.BasicQos(0, (ushort)_queue.Qos, false);

            _channel.BasicConsume(queue: _queue.QueueName, autoAck: false, consumer: consumer);

            while (!_queueBindCommands.IsEmpty)
            {
                _queueBindCommands.TryDequeue(out var command);

                switch (command?.Type)
                {
                    case QueueBindType.Bind:
                        _channel.QueueBind(queue: _queue.QueueName, exchange: _exchange.ExchangeName, routingKey: command.RoutingKey);
                        break;
                    case QueueBindType.Unbind:
                        _channel.QueueUnbind(queue: _queue.QueueName, exchange: _exchange.ExchangeName, routingKey: command.RoutingKey);
                        break;
                }
            }
        }

        public void OnMessageReceived(Func<IModel, BasicDeliverEventArgs, Task> callback)
        {
            _callbacks.Add(callback);
        }

        protected virtual async Task HandleIncomingMessage(IModel channel, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            try
            {
                foreach (var callback in _callbacks)
                {
                    await callback(channel, basicDeliverEventArgs);
                }

                channel.BasicAck(basicDeliverEventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ HandleIncomingMessage Error");
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.Dispose();
            }
            catch { }
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
