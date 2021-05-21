using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.RabbitMQ
{
    public class RabbitMqConsumer : IRabbitMqConsumer, IDisposable
    {
        private readonly ILogger<RabbitMqConsumer> _logger;
        private readonly IConnectionPool _connectionPool;
        private readonly SemaphoreSlim _connectionLock;
        private readonly ConcurrentBag<Func<IModel, BasicDeliverEventArgs, Task>> _callbacks;

        private ExchangeDeclareConfiguration _exchange;
        private QueueDeclareConfiguration _queue;

        private IModel _channel;

        public RabbitMqConsumer(ILogger<RabbitMqConsumer> logger, IConnectionPool connectionPool, ExchangeDeclareConfiguration exchange, QueueDeclareConfiguration queue)
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
            if (!IsConnected)
            {
                TryConnect();
            }

            _channel.QueueBind(queue: _queue.QueueName, exchange: _exchange.ExchangeName, routingKey: routingKey);
        }

        public void OnMessageReceived(Func<IModel, BasicDeliverEventArgs, Task> callback)
        {
            _callbacks.Add(callback);
        }

        public void Unbind(string routingKey)
        {
            if (!IsConnected)
            {
                TryConnect();
            }

            _channel.QueueUnbind(queue: _queue.QueueName, exchange: _exchange.ExchangeName, routingKey: routingKey);
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
    }
}
