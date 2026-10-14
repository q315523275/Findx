# Findx.WebApiClient

WebApiClient 组件，声明式远端接口请求，并扩展增加服务发现、熔断、降级、超时等功能，实现配置设置优先策略。

> Nuget 包引用

```
Findx.WebApiClient
```

目标框架：`net6.0` / `net8.0` / `net10.0`，底层依赖 `WebApiClientCore 2.1.6`。

> 本包无独立的 DI 扩展方法

通过 `StartupModule` 机制自动装配：

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 WebApiClientModule
```

`WebApiClientModule`（`ModuleLevel.Application`，`Order = 50`）启动时扫描所有标注 `[WebApiClient]` 的接口，为每个接口注册 HttpApi 并装配服务发现、负载均衡、熔断、降级、重试、超时管道。

## 声明远端接口

> 接口定义

```csharp
using Findx.WebApiClient;
using WebApiClientCore.Attributes;

[WebApiClient("http://OrderService", UseDiscovery = true, LoadBalancerType = LoadBalancerType.RoundRobin,
              Timeout = 5, Retry = 2)]
public interface IOrderApi
{
    [HttpGet("/api/orders/{id}")]
    Task<OrderDto> GetAsync([PathVar("id")] int id, CancellationToken cancellationToken = default);

    [HttpPost("/api/orders")]
    Task<CommonResult> CreateAsync([Body] OrderDto order, CancellationToken cancellationToken = default);
}
```

`[WebApiClient]` 继承自 WebApiClientCore 的 `HttpHostAttribute`，方法上的 `[HttpGet]` / `[HttpPost]` 等特性均来自 `WebApiClientCore.Attributes`。

> 特性属性

```csharp
[AttributeUsage(AttributeTargets.Interface)]
public class WebApiClientAttribute : HttpHostAttribute
{
    public string Name { set; get; }                                     // 名称
    public int Timeout { set; get; }                                     // 超时秒数，0 = 不启用
    public int Retry { set; get; }                                       // 重试次数，0 = 不启用
    public int FallbackStatus { set; get; }                              // 降级状态码，0 = 不启用
    public object FallbackMessage { set; get; }                          // 降级内容
    public int ExceptionsAllowedBeforeBreaking { set; get; }             // 熔断前连续错误次数
    public string DurationOfBreak { set; get; }                          // 熔断时长，如 "30s"
    public bool UseDiscovery { set; get; }                               // 是否使用服务发现
    public LoadBalancerType LoadBalancerType { set; get; } = LoadBalancerType.Random;
}
```

> 调用

```csharp
public class OrderService
{
    private readonly IOrderApi _api;
    public OrderService(IOrderApi api) => _api = api;   // 直接注入，无需手工注册

    public async Task<OrderDto> GetAsync(int id, CancellationToken ct)
        => await _api.GetAsync(id, ct);
}
```

## 配置设置优先

这是本包的核心设计：`WebApiClientOptions`（配置）全为**可空**类型，与 `WebApiClientAttribute`（代码特性）的非空类型形成「可空覆盖」配对。

> Json配置

配置节路径为 `Findx:WebApiClient:{接口类型名}`（`type.Name`，不含命名空间）。

```json
{
    "Findx": {
        "Discovery": {
            "Enabled": true
        },
        "WebApiClient": {
            "IOrderApi": {
                "UseDiscovery": true,
                "LoadBalancerType": "LeastConnection",
                "Timeout": 10,
                "Retry": 3,
                "ExceptionsAllowedBeforeBreaking": 5,
                "DurationOfBreak": "5s",
                "FallbackStatus": 200,
                "FallbackMessage": "{\"code\":500,\"message\":\"服务繁忙\"}"
            }
        }
    },
    "ServiceEndPoint": {
        "OrderService": [
            "http://10.0.0.11:5000",
            "http://10.0.0.12:5000"
        ]
    }
}
```

> 配置项

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Name` | `string?` | `null` | 名称 |
| `Timeout` | `int?` | `null` | 超时秒数 |
| `Retry` | `int?` | `null` | 重试次数 |
| `FallbackStatus` | `int?` | `null` | 降级状态码 |
| `FallbackMessage` | `string?` | `null` | 降级内容 |
| `ExceptionsAllowedBeforeBreaking` | `int?` | `null` | 熔断前连续错误次数 |
| `DurationOfBreak` | `string?` | `null` | 熔断时长 |
| `UseDiscovery` | `bool?` | `null` | 是否使用服务发现 |
| `LoadBalancerType` | `LoadBalancerType` | `Random` | 负载均衡算法（非可空） |

> 优先级规则

每一项都遵循同一套三态判定：

| 配置状态 | 行为 |
|---|---|
| 配置有值且 `> 0` | 使用配置值 |
| 配置有值且 `<= 0` | **显式关闭**该策略（特性里的值也一并禁用） |
| 配置无值（`null`） | 回落到特性值 |

上例中：超时由特性 `5` 改为配置 `10`，重试由 `2` 改为 `3`，负载均衡由 `RoundRobin` 改为 `LeastConnection`，并额外开启熔断与降级。节内未写的键为 `null`，继续回落到特性值。

## 装配的管道

对每个扫描到的接口按以下顺序装配：

| 步骤 | 行为 |
|---|---|
| 1 | `services.AddHttpApi(type)` 构建 HttpApi |
| 2 | 存在 `Findx:WebApiClient:{type.Name}` 配置节时绑定并 `ConfigureHttpApi(section)` |
| 3 | 默认 JSON 设置：`PropertyNameCaseInsensitive = true`、`PropertyNamingPolicy = null` |
| 4 | 降级策略 |
| 5 | 熔断策略 |
| 6 | 重试策略 |
| 7 | 超时策略 |
| 8 | 服务发现 + 负载均衡 |

服务发现分支根据 `LoadBalancerType` 选择对应的负载均衡扩展方法（`AddRandomLoadBalancer` / `AddRoundRobinLoadBalancer` / `AddLeastConnectLoadBalancer` / `AddIpHashLoadBalancer`），未匹配时默认随机。

## 服务发现配合

启用 `UseDiscovery` 时，接口声明中的 host 必须是**服务名**，由 [Findx.Discovery](Findx.Discovery.md) 解析为真实地址。

```json
{
    "Findx": {
        "Discovery": { "Enabled": true }
    },
    "ServiceEndPoint": {
        "OrderService": [
            "http://10.0.0.11:5000",
            "http://10.0.0.12:5000"
        ]
    }
}
```

```csharp
[WebApiClient("http://OrderService", UseDiscovery = true, LoadBalancerType = LoadBalancerType.IpHash)]
public interface IOrderApi { }
```

## 注意事项

- `Findx:Discovery:Enabled` 必须显式配置为 `true`，否则服务发现不会注册，`UseDiscovery` 不会生效。
- `AddLeastConnectLoadBalancer()` 当前源码实际使用的是轮询算法。
- 降级、熔断、重试、超时的具体策略实现见 [Findx.AspNetCore.md](Findx.AspNetCore.md#httpclient-策略)。