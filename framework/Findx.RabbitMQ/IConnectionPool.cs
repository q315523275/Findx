using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Findx.RabbitMQ;

/// <summary>
///     连接池服务接口
/// </summary>
public interface IConnectionPool : IAsyncDisposable
{
    /// <summary>
    ///     获取连接
    /// </summary>
    /// <param name="connectionName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IConnection> GetAsync(string connectionName = null, CancellationToken cancellationToken = default);
}