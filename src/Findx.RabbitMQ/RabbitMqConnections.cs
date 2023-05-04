using System.Collections.Generic;
using JetBrains.Annotations;
using RabbitMQ.Client;

namespace Findx.RabbitMQ
{
    public class RabbitMqConnections : Dictionary<string, ConnectionFactory>
    {
        public const string DefaultConnectionName = "Default";

        public RabbitMqConnections()
        {
            Default = new ConnectionFactory();
        }

        [NotNull]
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
}