﻿using System;
using System.ComponentModel;
using Findx.Caching;
using Findx.Extensions;
using Findx.Locks;
using Findx.Modularity;
using Findx.Redis.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Redis;

/// <summary>
///     Findx-Redis缓存模块
/// </summary>
[Description("Findx-Redis缓存模块")]
public class RedisModule : StartupModule
{
    /// <summary>
    ///     模块等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Framework;

    /// <summary>
    ///     模块排序
    /// </summary>
    public override int Order => 110;
        
    /// <summary>
    ///     配置模块服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // 配置服务
        var configuration = services.GetConfiguration();
        if (!configuration.GetValue<bool>("Findx:Redis:Enabled"))
            return services;
            
        // 配置参数
        services.Configure<RedisOptions>(configuration.GetSection("Findx:Redis"));

        // redis 连接池
        services.AddSingleton<IConnectionProvider, ConnectionProvider>();

        // redisClient 提供器
        services.AddSingleton<IRedisClientProvider, RedisClientProvider>();

        // redisClient 默认服务
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IRedisClientProvider>().CreateClient();
            return client;
        });

        // 存储序列化
        services.AddSingleton<IRedisSerializer, RedisSerializer>();

        // 分布式锁
        services.AddSingleton<ILock, RedisLock>();

        // 缓存实现
        services.AddSingleton<ICache, RedisCache>();

        return services;
    }

    /// <summary>
    /// 启用模块
    /// </summary>
    /// <param name="app"></param>
    public override void UseModule(IServiceProvider app)
    {
        var configuration = app.GetRequiredService<IConfiguration>();
        if (configuration.GetValue<bool>("Findx:Redis:Enabled"))
            base.UseModule(app);
    }
}