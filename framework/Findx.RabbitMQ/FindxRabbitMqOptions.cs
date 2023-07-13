using Microsoft.Extensions.Options;

namespace Findx.RabbitMQ;

public class FindxRabbitMqOptions: IOptions<FindxRabbitMqOptions>
{
    public RabbitMqConnections Connections { get; set; } = new();

    public bool Enabled { get; set; } = true;
    
    public FindxRabbitMqOptions Value => this;
}