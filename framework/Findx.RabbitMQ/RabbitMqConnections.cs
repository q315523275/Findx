using System.Collections.Generic;
using Findx.Common;
using RabbitMQ.Client;

namespace Findx.RabbitMQ;

public class RabbitMqConnections : Dictionary<string, ConnectionFactory>
{
    public const string DefaultConnectionName = "Default";

    public RabbitMqConnections()
    {
        Default = new ConnectionFactory();
    }
    
    public ConnectionFactory Default
    {
        get => this[DefaultConnectionName];
        set => this[DefaultConnectionName] = Check.NotNull(value, nameof(value));
    }

    public ConnectionFactory GetOrDefault(string connectionName)
    {
        if (TryGetValue(connectionName, out var connectionFactory)) return connectionFactory;

        return Default;
    }
}