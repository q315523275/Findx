using Findx.Caching;
using Findx.Redis;
using Findx.Redis.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.UnitTests;

/// <summary>
///     RedisModule 配置节回归测试（修复：误读 "Findx:FreeSql" 节导致 Redis 开关不生效）
/// </summary>
public class RedisModuleTests
{
    private static ServiceCollection BuildServices(IDictionary<string, string> config)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(config).Build();
        services.AddSingleton<IConfiguration>(configuration);
        return services;
    }

    [Fact]
    public void ConfigureServices_RedisDisabled_DoesNotRegisterRedisServices()
    {
        var services = BuildServices(new Dictionary<string, string>
        {
            ["Findx:Redis:Enabled"] = "false",
            // 干扰项：FreeSql 关闭不应误伤 Redis（修复前的复制粘贴 bug 会读这个节）
            ["Findx:FreeSql:Enabled"] = "true"
        });

        new RedisModule().ConfigureServices(services);

        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IConnectionProvider));
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(ICache));
    }

    [Fact]
    public void ConfigureServices_RedisEnabled_RegistersRedisServices()
    {
        var services = BuildServices(new Dictionary<string, string>
        {
            ["Findx:Redis:Enabled"] = "true"
        });

        new RedisModule().ConfigureServices(services);

        Assert.Contains(services, d => d.ServiceType == typeof(IConnectionProvider));
        Assert.Contains(services, d => d.ServiceType == typeof(IRedisClientProvider));
    }
}
