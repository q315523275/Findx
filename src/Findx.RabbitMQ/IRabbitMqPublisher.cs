namespace Findx.RabbitMQ
{
    public interface IRabbitMQPublisher
    {
        void Publish(object obj, string exchangeName, string exchangeType, string routingKey);
    }
}
