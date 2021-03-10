using Findx.Caching;
using Findx.Extensions;
using Findx.Locks;
using Findx.Modularity;
using Findx.Redis.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Findx.Redis
{
    [Description("Findex-Redis缓存模块")]
    public class RedisCacheModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;

        public override int Order => 20;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            IConfiguration configuration = services.GetConfiguration();
            var section = configuration.GetSection("Findx:Redis");
            services.Configure<RedisCacheOptions>(section);

            // redis 连接池
            services.AddSingleton<IConnectionPool, ConnectionPool>();

            // redisClient 提供器
            services.AddSingleton<IRedisClientProvider, StackExchangeRedisClientProvider>();

            // 存储序列化
            services.AddSingleton<IRedisSerializer, RedisJsonSerializer>();

            // 分布式锁
            services.AddSingleton<IDistributedLock, RedisDistributedLock>();

            // 缓存实现
            services.AddSingleton<ICache, RedisCacheProvider>();

            return services;
        }
    }
}
