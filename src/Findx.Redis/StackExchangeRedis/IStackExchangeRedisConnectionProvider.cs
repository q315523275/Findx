using StackExchange.Redis;
using System.Collections.Generic;

namespace Findx.Redis
{
    public interface IStackExchangeRedisConnectionProvider
    {
        /// <summary>
        /// 获取 Redis多连接复用器
        /// </summary>
        /// <param name="connectionName">连接信息名</param>
        /// <returns></returns>
        ConnectionMultiplexer GetConnection(string connectionName = null);

        /// <summary>
        /// 获取 服务器列表
        /// </summary>
        /// <param name="connectionName">连接信息名</param>
        /// <returns></returns>
        IEnumerable<IServer> GetServerList(string connectionName = null);

        /// <summary>
        /// 获取当前Redis服务器
        /// </summary>
        /// <param name="hostAndPort">主机名和端口</param>
        /// <param name="connectionName">连接信息名</param>
        /// <returns></returns>
        IServer GetServer(string hostAndPort, string connectionName = null);
    }
}
