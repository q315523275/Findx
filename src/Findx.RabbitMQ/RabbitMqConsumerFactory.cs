using Microsoft.Extensions.Logging;

namespace Findx.RabbitMQ
{
    public class RabbitMQConsumerFactory : IRabbitMQConsumerFactory
    {
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IConnectionPool _connectionPool;

        public RabbitMQConsumerFactory(ILogger<RabbitMQConsumer> logger, IConnectionPool connectionPool)
        {
            _logger = logger;
            _connectionPool = connectionPool;
        }

        public IRabbitMQConsumer Create(ExchangeDeclareConfiguration exchange, QueueDeclareConfiguration queue, bool autoAck = true)
        {
            return new RabbitMQConsumer(_logger, _connectionPool, exchange, queue, autoAck);
        }
    }
}
