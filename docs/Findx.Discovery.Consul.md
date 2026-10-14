# Findx.Discovery.Consul

Consul 组件，以 Consul 实现服务发现与注册功能。

> Nuget 包引用

```
Findx.Discovery.Consul
```

目标框架：`net6.0` / `net8.0` / `net10.0`，底层依赖 `Consul 1.8.0`。

本包为 [Findx.Discovery](Findx.Discovery.md) 提供 Consul 后端实现，包含实例注册（带健康检查）、注销、维护模式开关、健康实例查询。

> 装配方式

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 ConsulDiscoveryModule
```

`ConsulDiscoveryModule`（`ModuleLevel.Framework`，`Order = 100`）标注了 `[DependsOnModules([typeof(DiscoveryCoreModule)])]`，会自动引入基础模块并覆盖其默认的配置文件实现。

```csharp
services.TryAddSingleton<IConsulServiceRegistry, ConsulServiceRegistry>();
services.TryAddSingleton<IConsulRegistration, ConsulRegistration>();
services.TryAddSingleton<IServiceEndPointProvider, ConsulServiceEndPointProvider>();
services.TryAddSingleton(sp => ConsulClientFactory.CreateClient(sp.GetRequiredService<IOptionsMonitor<ConsulOptions>>().CurrentValue));
services.AddHostedService<ConsulDiscoveryAutoRegistryWorker>();
```

> 重要

开关仍是 `Findx:Discovery:Enabled`（不是 `Findx:Consul:Enabled`），且必须**显式配置为 `true`**。

## Json 配置

```json
{
    "Findx": {
        "Discovery": {
            "Enabled": true,
            "ServiceName": "Findx.WebHost",
            "Scheme": "http",
            "Port": 8888,
            "PreferIpAddress": true,
            "InstanceGroup": "hangzhou",
            "Tags": [ "region=hz", "env=dev" ],
            "RegisterHealthCheck": true,
            "HealthCheckPath": "/api/app/health",
            "HealthCheckInterval": "10s",
            "HealthCheckTimeout": "10s",
            "HealthCheckCriticalTimeout": "30m",
            "HealthCheckTlsSkipVerify": true,
            "Register": true,
            "Deregister": true
        },
        "Consul": {
            "Host": "127.0.0.1",
            "Port": 8500,
            "Scheme": "http",
            "Datacenter": "dc1",
            "Token": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
            "WaitTime": "30s"
        }
    }
}
```

`Findx:Discovery` 各项说明见 [Findx.Discovery.md](Findx.Discovery.md#json-配置)。

> Findx:Consul 配置

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `Host` | `localhost` | Consul 主机 |
| `Scheme` | `http` | 协议 |
| `Port` | `8500` | 端口 |
| `Datacenter` | 无 | 数据中心 |
| `Token` | 无 | ACL Token |
| `WaitTime` | 无 | 阻塞查询等待时间，如 `"30s"` |

## 服务注册

`ConsulDiscoveryAutoRegistryWorker` 在应用启动时注册一次实例，心跳由 Consul 服务端按注册的检查配置主动探测。

> 注册内容

| 项 | 值 |
|---|---|
| 实例 ID | `{ServiceName}-{Host}:{Port}`，其中 `.` 与 `:` 替换为 `-` |
| 服务名 | `Findx:Discovery:ServiceName`，为空时取 `IApplicationContext.ApplicationName` |
| 地址 | `Findx:Discovery:HostName` |
| 端口 | `Findx:Discovery:Port`，为 `0` 时取 `IApplicationContext.Port` |
| 标签 | 用户 `Tags` + `group={InstanceGroup}` + `secure={scheme==https}` + `version={应用版本}` |

`Metadata` 由 `key=value` 型标签拆解成字典（无 `=` 的标签以标签名作为 key）。

## 健康检查

> 默认地址

```
{Scheme}://{HostName}:{Port}{HealthCheckPath}
```

配置了 `HealthCheckUrl` 时优先使用该完整地址。

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `RegisterHealthCheck` | `true` | 是否注册健康检查 |
| `HealthCheckUrl` | 无 | 完整地址，优先于 Path |
| `HealthCheckPath` | `/health` | 路径 |
| `HealthCheckInterval` | `10s` | 探测间隔 |
| `HealthCheckTimeout` | `10s` | 探测超时 |
| `HealthCheckCriticalTimeout` | `30m` | 连续失败多久后移除实例 |
| `HealthCheckTlsSkipVerify` | `true` | 跳过 TLS 认证 |

> 注意事项

`ConsulDiscoveryModule` 的 `OnShutdown` 在应用停止时会**自动注销**（受 `Deregister` 开关控制），实现优雅下线。

## 手动控制上下线

```csharp
public class Maintenance
{
    private readonly IConsulServiceRegistry _registry;
    private readonly IConsulRegistration _registration;

    public Maintenance(IConsulServiceRegistry registry, IConsulRegistration registration)
    {
        _registry = registry;
        _registration = registration;
    }

    // 优雅下线：置为维护模式
    public Task OffLineAsync() => _registry.SetStatusAsync(_registration, "OUT_OF_SERVICE");

    // 恢复上线
    public Task OnLineAsync() => _registry.SetStatusAsync(_registration, "UP");

    // 查询状态
    public Task<string> GetAsync() => _registry.GetStatusAsync(_registration);
}
```

> 支持的状态

| 状态 | 说明 |
|---|---|
| `UP` | 正常服务，禁用维护模式 |
| `OUT_OF_SERVICE` | 维护模式，Consul 不再分发流量 |

> 注意事项

`IConsulClient` 是**单例**，按注册时的配置创建，运行期修改 `Findx:Consul` 配置**不会重建客户端**，需重启应用。

## 查询实例

`ConsulServiceEndPointProvider`（服务名 `Consul`）通过 `Health.Service` 查询健康实例列表。

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

> 注意事项

`GetServiceEndPointsAsync` 的 `group` / `tag` 参数在 Consul 实现中**未使用**。