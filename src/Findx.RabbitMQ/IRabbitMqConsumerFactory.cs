namespace Findx.RabbitMQ
{
    public interface IRabbitMQConsumerFactory
    {
        IRabbitMQConsumer Create(ExchangeDeclareConfiguration exchange, QueueDeclareConfiguration queue);
    }
}
