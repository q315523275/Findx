namespace Findx.RabbitMQ
{
    public interface IRabbitMqConsumerFactory
    {
        IRabbitMqConsumer Create(ExchangeDeclareConfiguration exchange, QueueDeclareConfiguration queue);
    }
}
