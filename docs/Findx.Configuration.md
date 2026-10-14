# Findx.Configuration

Configuration 组件，实现配置中心客户端，采用 Http 长轮询方式实现配置的热更新，实现 `IConfiguration` 读取配置零侵入。

> Nuget 包引用

```
Findx.Configuration
```

目标框架：`net6.0` / `net8.0` / `net10.0`。

服务端组件位于 `Findx.Extensions.ConfigurationServer`，模块化方式 Nuget 引用后直接使用。

## 核心特性

配置中心的数据被注入到原生 `IConfiguration` 体系，因此业务代码**零侵入**——仍然只用 `IConfiguration` / `IOptions<T>` 读取，但底层数据会在配置变更时自动触发 `OnReload()`。

> 配置变更传播链路

```
配置中心变更 → ConfigClient 回调 → 写入 ConfigProvider.Data → OnReload()
             → 所有绑定 IOptionsMonitor<T> 的选项自动刷新
```

这意味着 `Findx:Email`、`Findx:Mapping`、`Findx:Redis` 等所有配置节在配置中心改动后同样热生效。

## 使用方式

`AddFindxConfig` 提供四个重载。

> 无参重载（读配置节）

```csharp
using Findx.Configuration.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddFindxConfig();

builder.Services.AddFindx().AddModules();
```

> 委托重载（代码配置）

```csharp
builder.Configuration.AddFindxConfig(o =>
{
    o.AppId = "findx";
    o.Secret = "your-secret";
    o.Environment = "dev";
    o.Servers = "http://a:9010;http://b:9010";
});
```

> 自定义客户端

```csharp
builder.Configuration.AddFindxConfig(new MyConfigClient(options));
```

`IHostBuilder` 场景使用对应的 `UseFindxConfig(...)`，内部转调 `ConfigureAppConfiguration`。

## Json 配置

配置节为 **`Findx:Config`**。

```json
{
    "Findx": {
        "Config": {
            "AppId": "findx",
            "Secret": "your-secret",
            "Environment": "dev",
            "Servers": "http://127.0.0.1:9010",
            "IsRecovery": false
        }
    }
}
```

| 配置项 | 类型 | 必填 | 说明 |
|---|---|---|---|
| `Enabled` | `bool` | — | 是否启用，默认 `true` |
| `AppId` | `string` | 是 | 配置应用编号 |
| `Secret` | `string` | 是 | 应用密钥 |
| `Environment` | `string` | 是 | 环境标识 |
| `Servers` | `string` | 是 | 服务节点地址，多节点用 `;` 分隔 |
| `IsRecovery` | `bool` | — | 是否异常容灾恢复（本地备份），默认 `false` |

## 长轮询机制

> 请求地址

```
{CurrentServer}/api/config?appId={AppId}&sign={sign}&environment={Environment}&version={CurrentDataVersion}&load={请求异常次数}
```

签名：`sign = Md5By32($"{AppId}{AppSecret}{Environment}{CurrentDataVersion}")`

> 关键行为

| 项 | 说明 |
|---|---|
| 节点轮转 | `Servers` 按 `;` 拆分，用 `AtomicInteger` 轮询，避免单点故障 |
| 变更判定 | 响应为 `List<ConfigItem>`，仅当存在 `Version > 当前版本` 时才更新 |
| 长轮询启动 | 当前版本 `> 0` 时启动后台循环反复拉取 |
| 异常重试 | 轮询中异常等待 30 秒后重试 |
| HttpClient 调优 | 连接池生命周期 2 分钟（应对 K8s / 容器服务迁移导致 DNS 变化） |

## 容灾恢复

`IsRecovery = true` 时：

1. 配置变更后写入工作目录下的 `appsettings.ConfigCache.json` 本地备份
2. 首次请求异常且非轮询中时，读回该备份继续工作
3. 未开启容灾时，首次异常直接抛出

## 数据类型

```csharp
public enum DataType { Text, Json }
```

| 类型 | 处理方式 |
|---|---|
| `Text` | 内容原样作为配置值 |
| `Json` | 内容按 JSON 展开为 `a:b:c` 形式（递归展开，键分隔符为标准配置路径分隔符） |

## 注意事项

- `AddFindxConfig()` 无参重载会先 `Build()` 一次配置再读取 `Findx:Config` 节，需确保该节已存在于配置源中。
- `Enabled = false` 时不注册任何配置源，等同于未调用。
- 配置内容仅存在于内存与本地备份文件，应用重启后需要服务端仍在运行才能拉到最新配置。