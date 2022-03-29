using System;
namespace Findx.RabbitMQ
{
	public class FindxRabbitMqOptions
	{
        public RabbitMqConnections Connections { get; }

        public FindxRabbitMqOptions()
        {
            Connections = new RabbitMqConnections();
        }
    }
}

