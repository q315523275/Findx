using System.ComponentModel;
using Findx.Caching;
using Findx.Extensions;
using Findx.Locks;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Redis
{
    /// <summary>
    ///     Findx-Redis缓存模块
    /// </summary>
    [Description("Findx-Redis缓存模块")]
    public class FindxRedisModule : FindxModule
    {
        /// <summary>
        ///     模块等级
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Framework;

        /// <summary>
        ///     模块排序
        /// </summary>
        public override int Order => 40;

        /// <summary>
        ///     配置模块服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            var configuration = services.GetConfiguration();
            services.Configure<FindxRedisOptions>(configuration.GetSection("Findx:Redis"));

            // redis 连接池
            services.AddSingleton<IStackExchangeRedisConnectionProvider, StackExchangeRedisConnectionProvider>();

            // redisClient 提供器
            services.AddSingleton<IRedisClientProvider, StackExchangeRedisClientProvider>();

            // redisClient 默认服务
            services.AddSingleton(sp => sp.GetRequiredService<IRedisClientProvider>().CreateClient());

            // 存储序列化
            services.AddSingleton<IRedisSerializer, RedisJsonSerializer>();

            // 分布式锁
            services.AddSingleton<ILock, RedisDistributedLock>();

            // 缓存实现
            services.AddSingleton<ICache, RedisCacheProvider>();

            return services;
        }
    }
}