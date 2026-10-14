# Findx.Discovery

服务发现 组件，包含服务的发现与注册、内置负载算法（IP哈希一致性、最小连接数、随机、轮询），实现 HttpClient 的 `HttpMessageHandler` 负载端点计算减少代码侵入，默认实现配置文件方式的服务发现功能。

> Nuget 包引用

```
Findx.Discovery
```

目标框架：`net6.0` / `net8.0` / `net10.0`。

> 本包无独立的 DI 扩展方法

通过 `StartupModule` 机制自动装配：

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 DiscoveryCoreModule
```

`DiscoveryCoreModule` 按 `Findx:Discovery:Enabled` 开关决定是否注册相关服务。

> 重要

模块判定是否启用时读取的是 `IConfiguration` 原始值（`GetValue<bool>`），**缺省为 false**。虽然 `DiscoveryOptions.Enabled` 属性的默认值是 `true`，但必须显式配置 `"Enabled": true` 才会生效。

## Json 配置

```json
{
    "Findx": {
        "Discovery": {
            "Enabled": true,
            "ServiceName": "",
            "Scheme": "http",
            "Port": 0,
            "PreferIpAddress": true,
            "HostName": "",
            "IpAddress": "",
            "InstanceId": "",
            "InstanceGroup": "",
            "Tags": [],
            "Cache": false,
            "CacheStrategy": "memoryCache",
            "CacheTtl": 15,
            "RegisterHealthCheck": true,
            "HealthCheckUrl": "",
            "HealthCheckPath": "/health",
            "HealthCheckInterval": "10s",
            "HealthCheckTimeout": "10s",
            "HealthCheckCriticalTimeout": "30m",
            "HealthCheckTlsSkipVerify": true,
            "Register": true,
            "Deregister": true
        }
    }
}
```

> 配置说明

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `Enabled` | — | 是否启用服务发现，**必须显式配置** |
| `ServiceName` | 无 | 服务注册名称 |
| `Scheme` | `http` | 注册协议 |
| `Port` | `0` | 注册端口，`0` 时取 `IApplicationContext.Port` |
| `PreferIpAddress` | `true` | 用 IP 而非主机名注册 |
| `HostName` | 机器名 | `PreferIpAddress = true` 时该属性返回 IP |
| `IpAddress` | 解析出的地址 | 注册 IP |
| `InstanceId` | 无 | 实例全局唯一编号 |
| `InstanceGroup` | 无 | 实例组 |
| `Tags` | 无 | 服务标签 |
| `Cache` | `false` | 是否缓存实例列表 |
| `CacheStrategy` | `memoryCache` | 缓存策略（`memoryCache` / `redis.default`） |
| `CacheTtl` | `15` | 缓存时间（秒） |
| `RegisterHealthCheck` | `true` | 是否注册健康检查 |
| `HealthCheckUrl` | 无 | 健康检查完整地址，优先于 Path |
| `HealthCheckPath` | `/health` | 健康检查路径 |
| `HealthCheckInterval` | `10s` | 健康检查轮询时间 |
| `HealthCheckTimeout` | `10s` | 健康检查超时 |
| `HealthCheckCriticalTimeout` | `30m` | 探测失败后多久移除 |
| `HealthCheckTlsSkipVerify` | `true` | 健康检查跳过 TLS 认证 |
| `Register` / `Deregister` | `true` | 是否注册 / 注销 |

时间字符串仅支持 `ms` / `s` / `m` / `h` 后缀。

## 配置文件方式的服务发现

默认实现 `ConfigServiceEndPointProvider`（服务名 `Config`）读取**根级** `ServiceEndPoint` 配置节。

> ServiceEndPoint配置

```json
{
    "ServiceEndPoint": {
        "Findx.WebHost": [
            "http://127.0.0.1:8888",
            "http://127.0.0.1:8889"
        ],
        "OrderService": [
            "http://10.0.0.11:5000",
            "http://10.0.0.12:5000"
        ]
    }
}
```

每条端点值必须是能被 `new Uri(...)` 解析的完整地址。

> 使用

```csharp
builder.Services.AddFindx().AddModules();

// 请求 http://OrderService/api/orders，实际会打到 10.0.0.11 或 10.0.0.12
builder.Services.AddHttpClient("order", c => c.BaseAddress = new Uri("http://OrderService"))
                .AddRoundRobinLoadBalancer();
```

```csharp
public class OrderService
{
    private readonly IHttpClientFactory _factory;
    public OrderService(IHttpClientFactory factory) => _factory = factory;

    public async Task<string> GetAsync(CancellationToken ct)
        => await _factory.CreateClient("order").GetStringAsync("/api/orders", ct);
}
```

> 注意事项

- 配置节是根级 `ServiceEndPoint`，**不在** `Findx:Discovery` 下。
- `IServiceEndPointProvider` 的 `group` / `passingOnly` / `tag` 参数是给注册中心预留的，配置文件实现会忽略。

## 核心抽象

```csharp
public interface IServiceEndPoint
{
    string ServiceName { get; }
    string Host { get; }
    int Port { get; }
    IDictionary<string, string> Metadata { get; }
}
```

```csharp
public interface IServiceEndPointProvider
{
    Task<IReadOnlyList<IServiceEndPoint>> GetServiceEndPointsAsync(
        string serviceName, string group = null, bool passingOnly = true, string tag = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetServicesAsync(string group = null, CancellationToken cancellationToken = default);
}
```

```csharp
public interface IDiscoveryClient : IServiceEndPointProvider
{
    string ProviderName { get; }
    Task<IList<IServiceEndPoint>> GetAllEndPointsAsync(string group = null, CancellationToken cancellationToken = default);
}
```

> 使用

```csharp
public class DiscoveryQuery
{
    private readonly IDiscoveryClient _client;
    public DiscoveryQuery(IDiscoveryClient client) => _client = client;

    public async Task<object> RunAsync(CancellationToken ct)
    {
        var all = await _client.GetAllEndPointsAsync(cancellationToken: ct);
        var orderIns = await _client.GetServiceEndPointsAsync("OrderService", cancellationToken: ct);
        return new { Provider = _client.ProviderName, All = all, Order = orderIns };
    }
}
```

`DiscoveryClient` 在 `Cache = true` 时会用 `ICacheFactory` 缓存实例列表，缓存键前缀为 `ServiceEndPoints:`。

## 负载均衡

内置 4 种算法选择器。

| 扩展方法 | 枚举值 | 算法 |
|---|---|---|
| `AddRandomLoadBalancer()` | `LoadBalancerType.Random` | 随机 |
| `AddRoundRobinLoadBalancer()` | `LoadBalancerType.RoundRobin` | 轮询（`Interlocked.Increment`） |
| `AddLeastConnectLoadBalancer()` | `LoadBalancerType.LeastConnection` | 最小连接数 |
| `AddIpHashLoadBalancer()` | `LoadBalancerType.IpHash` | **一致性哈希** |

```csharp
public enum LoadBalancerType
{
    Random, RoundRobin, LeastConnection, IpHash, NoLoadBalancer
}
```

> 直接使用负载均衡器

```csharp
public class BalancerQuery
{
    private readonly ILoadBalancerProvider _provider;
    public BalancerQuery(ILoadBalancerProvider provider) => _provider = provider;

    public async Task<IServiceEndPoint> ResolveAsync(string serviceName, CancellationToken ct)
    {
        var balancer = await _provider.GetAsync(serviceName, LoadBalancerType.IpHash);
        return await balancer.ResolveServiceEndPointAsync(ct);
    }
}
```

> 工作原理

四个 `DelegatingHandler` 在请求发出前完成 URL 改写，请求结束后还原 URL 供管道复用：

```csharp
var loadBalancer = await _loadBalancerManager.GetAsync(current.Host, type);
var serviceInfo = await loadBalancer.ResolveServiceEndPointAsync();
request.RequestUri = serviceInfo.ToUri(current.Scheme, current.PathAndQuery);   // 改写

try { return await base.SendAsync(request, cancellationToken); }
finally
{
    request.RequestUri = current;                                                 // 还原
    await loadBalancer.UpdateStatsAsync(serviceInfo, DateTime.Now - nowTime);
}
```

> 注意事项

- URL 改写以 `RequestUri.Host` 作为**服务名**，因此请求的 host 必须是服务名而非真实地址；scheme 沿用原请求，`PathAndQuery` 保持不变。
- `AddLeastConnectLoadBalancer()` 当前源码实际使用的是 `RoundRobin` 算法，最小连接数算法不会被启用。
- `IpHash` 使用一致性哈希环，客户端 IP 取自 `IThreadCurrentClientIpAccessor`，取不到时回退本机 IP；哈希一致性说明见 [ConsistentHash.md](ConsistentHash.md)。
- 实例列表为空时各选择器会抛 `ArgumentNullException`。

## 弹性策略

配合 `Findx.AspNetCore` 提供的 Polly 扩展使用：

```csharp
builder.Services.AddHttpClient("policy")
                .AddFallbackPolicy(CommonResult.Fail(), 200)
                .AddCircuitBreakerPolicy(5, "5s")
                .AddRetryPolicy(1)
                .AddTimeoutPolicy(1);
```

详细说明见 [Findx.AspNetCore.md](Findx.AspNetCore.md#httpclient-策略)。