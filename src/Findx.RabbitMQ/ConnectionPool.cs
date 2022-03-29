using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Findx.Extensions;
namespace Findx.RabbitMQ
{
    public class ConnectionPool : IConnectionPool, IDisposable
    {

        protected FindxRabbitMqOptions Options { get; }

        protected ConcurrentDictionary<string, Lazy<IConnection>> Connections { get; }

        private bool _isDisposed;

        public ConnectionPool(IOptions<FindxRabbitMqOptions> options)
        {
            Options = options.Value;
            Connections = new ConcurrentDictionary<string, Lazy<IConnection>>();
        }

        public virtual IConnection Get(string connectionName = null)
        {
            connectionName ??= RabbitMqConnections.DefaultConnectionName;

            try
            {
                var lazyConnection = Connections.GetOrAdd(
                    connectionName, (connectionName) => new Lazy<IConnection>(() =>
                    {
                        var connection = Options.Connections.GetOrDefault(connectionName);
                        var hostnames = connection.HostName.TrimEnd(';').Split(';');
                        // Handle Rabbit MQ Cluster.
                        return hostnames.Length == 1 ? connection.CreateConnection() : connection.CreateConnection(hostnames);

                    })
                );

                return lazyConnection.Value;
            }
            catch (Exception)
            {
                Connections.TryRemove(connectionName, out _);
                throw;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            foreach (var connection in Connections.Values)
            {
                try
                {
                    connection.Value.Dispose();
                }
                catch
                {

                }
            }

            Connections.Clear();
        }
    }
}
