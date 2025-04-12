using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Findx.Locks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Findx.RabbitMQ;

/// <summary>
///     连接池服务
/// </summary>
public class ConnectionPool : IConnectionPool
{
    private bool _isDisposed;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    public ConnectionPool(IOptions<FindxRabbitMqOptions> options)
    {
        Options = options.Value;
        Connections = new ConcurrentDictionary<string, IConnection>();
    }

    /// <summary>
    ///     Options配置信息
    /// </summary>
    protected FindxRabbitMqOptions Options { get; }

    /// <summary>
    ///     连接字典
    /// </summary>
    protected ConcurrentDictionary<string, IConnection> Connections { get; }
    
    /// <summary>
    ///     异步锁
    /// </summary>
    protected readonly AsyncLock AsyncLock = new();

    /// <summary>
    ///     获取连接
    /// </summary>
    /// <param name="connectionName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IConnection> GetAsync(string connectionName = null, CancellationToken cancellationToken = default)
    {
        using (await AsyncLock.LockAsync(cancellationToken))
        {
            connectionName ??= RabbitMqConnections.DefaultConnectionName;

            if (Connections.TryGetValue(connectionName, out var existingConnection) && existingConnection.IsOpen)
            {
                return existingConnection;
            }

            if(existingConnection != null)
            {
                await existingConnection.DisposeAsync();
            }

            var connectionFactory = Options.Connections.GetOrDefault(connectionName);
            var connection = await GetConnectionAsync(connectionName, connectionFactory, cancellationToken);
            Connections[connectionName] = connection;
            return connection;
        }
    }

    /// <summary>
    ///     获取连接接口
    /// </summary>
    /// <param name="connectionName"></param>
    /// <param name="connectionFactory"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<IConnection> GetConnectionAsync(string connectionName, ConnectionFactory connectionFactory, CancellationToken cancellationToken = default)
    {
        var hostnames = connectionFactory.HostName.TrimEnd(';').Split(';');
        return hostnames.Length == 1 ? await connectionFactory.CreateConnectionAsync(cancellationToken) : await connectionFactory.CreateConnectionAsync(hostnames, cancellationToken);
    }
    
    /// <summary>
    ///     异步释放资源
    /// </summary>
    public async ValueTask DisposeAsync()
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
                await connection.DisposeAsync();
            }
            catch
            {
                // ignored
            }
        }

        Connections.Clear();
    }
}