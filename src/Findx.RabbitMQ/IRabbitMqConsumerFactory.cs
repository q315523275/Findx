namespace Findx.RabbitMQ
{
    /// <summary>
    /// RabbitMQ消费者工厂
    /// </summary>
    public interface IRabbitMQConsumerFactory
    {
        /// <summary>
        /// 创建消费者
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <param name="autoAck"></param>
        /// <returns></returns>
        IRabbitMQConsumer Create(ExchangeDeclareConfiguration exchange, QueueDeclareConfiguration queue, bool autoAck = true);
    }
}
