using Microsoft.Extensions.Logging;

namespace Findx.RabbitMQ
{
    public class RabbitMqConsumerFactory : IRabbitMqConsumerFactory
    {
        private readonly ILogger<RabbitMqConsumer> _logger;
        private readonly IConnectionPool _connectionPool;

        public RabbitMqConsumerFactory(ILogger<RabbitMqConsumer> logger, IConnectionPool connectionPool)
        {
            _logger = logger;
            _connectionPool = connectionPool;
        }

        public IRabbitMqConsumer Create(ExchangeDeclareConfiguration exchange, QueueDeclareConfiguration queue)
        {
            return new RabbitMqConsumer(_logger, _connectionPool, exchange, queue);
        }
    }
}
