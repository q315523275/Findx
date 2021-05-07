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
    public class RedisModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;

        public override int Order => 20;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            IConfiguration configuration = services.GetConfiguration();
            var section = configuration.GetSection("Findx:Redis");
            services.Configure<RedisOptions>(section);

            // redis 连接池
            services.AddSingleton<IStackExchangeRedisDataBaseProvider, StackExchangeRedisDataBaseProvider>();

            // redisClient 提供器
            services.AddSingleton<IRedisClientProvider, StackExchangeRedisClientProvider>();

            // redisClient 默认服务
            services.AddSingleton(sp => sp.GetRequiredService<IRedisClientProvider>().CreateClient());

            // 存储序列化
            services.AddSingleton<IRedisSerializer, RedisJsonSerializer>();

            // 分布式锁
            services.AddSingleton<IDistributedLock, RedisDistributedLock>();

            // 缓存实现
            services.AddSingleton<ICache, RedisCacheProvider>();

            // redis队列
            services.AddSingleton<IRedisMqPublisherProvider, StackExchangeRedisMqPublisherProvider>();
            services.AddSingleton(sp => sp.GetRequiredService<IRedisMqPublisherProvider>().Create());
            services.AddSingleton<IRedisMqConsumerProvider, StackExchangeRedisMqConsumerProvider>();

            return services;
        }
    }
}
