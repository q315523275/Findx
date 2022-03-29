﻿namespace Findx.Redis
{
    public interface IRedisClientProvider
    {
        /// <summary>
        /// 创建redis客户端服务
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        IRedisClient CreateClient(string connectionName = null);

        /// <summary>
        /// 创建redis客户端服务
        /// </summary>
        /// <param name="redisSerializer"></param>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        IRedisClient CreateClient(IRedisSerializer redisSerializer, string connectionName = null);
    }
}
