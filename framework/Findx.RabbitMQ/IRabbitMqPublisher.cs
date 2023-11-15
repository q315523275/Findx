namespace Findx.RabbitMQ
{
    public interface IRabbitMqPublisher
    {
        void Publish(object obj, string exchangeName, string exchangeType, string routingKey, bool durable = false, bool autoDelete = false);
    }
}