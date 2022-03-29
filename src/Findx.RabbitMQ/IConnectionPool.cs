using System;
using RabbitMQ.Client;

namespace Findx.RabbitMQ
{
    public interface IConnectionPool : IDisposable
    {
        IConnection Get(string connectionName = null);
    }
}
