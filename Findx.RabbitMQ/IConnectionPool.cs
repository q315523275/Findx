using RabbitMQ.Client;

namespace Findx.RabbitMQ
{
    public interface IConnectionPool
    {
        IConnection Acquire();
    }
}
