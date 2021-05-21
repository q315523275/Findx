using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Redis.StackExchangeRedis
{
    public interface IStackExchangeRedisDataBaseProvider
    {
        /// <summary>
        /// 获取 Redis多连接复用器
        /// </summary>
        /// <param name="options">连接字符串</param>
        /// <returns></returns>
        ConnectionMultiplexer GetConnection(RedisOptions options);
        /// <summary>
        /// 获取 Redis多连接复用器
        /// </summary>
        /// <param name="options">连接字符串</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ConnectionMultiplexer> GetConnectionAsync(RedisOptions options, CancellationToken cancellationToken = default);
        /// <summary>
        /// 获取 服务器列表
        /// </summary>
        /// <param name="options">连接字符串</param>
        /// <returns></returns>
        IEnumerable<IServer> GetServerList(RedisOptions options);
        /// <summary>
        /// 获取当前Redis服务器
        /// </summary>
        /// <param name="hostAndPort">主机名和端口</param>
        /// <param name="options">连接字符串</param>
        /// <returns></returns>
        IServer GetServer(string hostAndPort, RedisOptions options);
    }
}
