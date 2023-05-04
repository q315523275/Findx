namespace Findx.RabbitMQ
{
    /// <summary>
    ///     RabbitMQ消费者工厂
    /// </summary>
    public interface IRabbitMqConsumerFactory
    {
        /// <summary>
        ///     创建消费者
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <param name="autoAck"></param>
        /// <returns></returns>
        IRabbitMqConsumer Create(ExchangeDeclareConfiguration exchange, QueueDeclareConfiguration queue,
            string connectionName = null);
    }
}