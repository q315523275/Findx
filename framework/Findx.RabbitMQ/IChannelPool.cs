using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.RabbitMQ;

/// <summary>
///     虚拟连接池服务接口
/// </summary>
public interface IChannelPool: IAsyncDisposable
{
    /// <summary>
    ///     获取虚拟连接服务接口
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="connectionName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IChannelAccessor> AcquireAsync(string channelName = null, string connectionName = null, CancellationToken cancellationToken = default);
}