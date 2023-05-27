namespace Findx.RabbitMQ
{
    public class FindxRabbitMqOptions
    {
        public FindxRabbitMqOptions()
        {
            Connections = new RabbitMqConnections();
        }

        public RabbitMqConnections Connections { get; }
    }
}