# Findx.NLog

NLog 组件，用 NLog 实现日志的输出与记录，内置配置文件。

> Nuget 包引用

```
Findx.NLog
```

目标框架：`net6.0` / `net8.0` / `net10.0`，底层依赖 `NLog 6.2.0`。

## 装配方式

> **不调用 `AddNLog()`**

本包自行实现 `ILoggerProvider` 并通过 `Replace` 覆盖默认 Provider。

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 NLogModule
```

`NLogModule`（`ModuleLevel.Framework`，`Order = 90`）：

```csharp
var configuration = services.GetConfiguration();
if (!configuration.GetValue<bool>("Logging:Enabled")) return services;   // 开关

services.Replace(new ServiceDescriptor(
    typeof(ILoggerProvider), typeof(NLogLoggerProvider), ServiceLifetime.Singleton));

LogManager.Setup().SetupExtensions(builder =>
{
    builder.RegisterLayoutRenderer<TraceIdentifierLayoutRenderer>("TraceIdentifier");
});
```

> 重要

开关为 `Logging:Enabled`，读取的是 `IConfiguration` 原始值，**必须显式配置为 `true`**，否则不注册任何东西。

## Json 配置

```json
{
    "Logging": {
        "Enabled": true,
        "LogLevel": {
            "Default": "Information",
            "Microsoft": "Warning"
        }
    }
}
```

> 注意事项

- `Logging` 是**根级**配置节，不在 `Findx` 下（与 `Findx:Auditing` 等框架配置节不同）。
- `Logging:Enabled` 缺省为 false 时 `NLogModule` 完全不生效。
- `Replace(ILoggerProvider)` 会**替换所有 Provider**，若同时使用其它日志组件需注意模块 `Order` 的执行顺序。

## 内置配置文件

包内 `nlog.config` 会自动复制到输出目录。

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      xmlns="http://www.nlog-project.org/schemas/NLog.xsd">

    <variable name="logLayout"
              value="[${longdate}][${uppercase:${level}}][${TraceIdentifier}][${threadid}] ${message} ${onexception:${exception:format=toString}}"/>

    <targets async="true">
        <target xsi:type="File" name="log_file"
                fileName="${basedir}/logs/${shortdate}/${shortdate}.txt"
                layout="${logLayout}"
                archiveFileName="${basedir}/archives/${shortdate}-{#####}.txt"
                archiveAboveSize="102400"
                archiveNumbering="Sequence"
                concurrentWrites="false"
                keepFileOpen="true"
                encoding="utf-8"
                openFileCacheTimeout="30" />
    </targets>

    <rules>
        <logger name="*" minlevel="Info" writeTo="log_file" />
    </rules>
</nlog>
```

> 配置说明

| 项 | 值 |
|---|---|
| 日志文件 | `${basedir}/logs/{日期}/{日期}.txt` |
| 归档文件 | `${basedir}/archives/{日期}-{序号}.txt` |
| 归档阈值 | 超过 100 KB 滚动 |
| Target 模式 | 异步写入（`async="true"`） |
| 编码 | UTF-8 |
| 最低级别 | `Info` |

> 日志行示例

```
[2026-10-14 10:00:00.0000][INFO][3a135b83-a6c1-32da-b7f3-66272cfbc8ad][17] 消息内容
```

## 自定义布局渲染器

`TraceIdentifierLayoutRenderer` 输出链路追踪 Id，优先级如下：

| 优先级 | 来源 |
|---|---|
| 1 | `ICorrelationIdProvider.Get()`（由 `app.UseCorrelationId()` 建立） |
| 2 | `HttpContext.TraceIdentifier`（ASP.NET Core 原生） |
| 3 | 空字符串 |

```csharp
[LayoutRenderer("TraceIdentifier")]
public class TraceIdentifierLayoutRenderer : LayoutRenderer
{
    protected override void Append(StringBuilder builder, LogEventInfo logEvent)
    {
        var correlationIdProvider = ServiceLocator.GetService<ICorrelationIdProvider>();
        var httpContextAccessor = ServiceLocator.GetService<IHttpContextAccessor>();

        if (correlationIdProvider != null && correlationIdProvider.Get().IsNotNullOrWhiteSpace())
            builder.Append(correlationIdProvider.Get());
        else if (httpContextAccessor is { HttpContext: not null })
            builder.Append(httpContextAccessor.HttpContext.TraceIdentifier);
    }
}
```

> 注意事项

要让 `${TraceIdentifier}` 输出 Findx 的关联 Id（而非 ASP.NET Core 的 TraceIdentifier），需在管道中启用：

```csharp
app.UseCorrelationId().UseFindx();
```

## 使用

业务代码中正常使用标准的 `ILogger<T>` 即可。

```csharp
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    public OrderController(ILogger<OrderController> logger) => _logger = logger;

    public IActionResult Create()
    {
        _logger.LogInformation("创建订单 {OrderId}", 1001);
        return Ok();
    }
}
```

## 注意事项

- `nlog.config` 缺失时会抛 `FileNotFoundException`，需要确保文件复制到输出目录。
- 日志级别映射：`Information → Info`、`Warning → Warn`、`Critical → Fatal`，未知级别降级为 `Info` 并打一条 Warning。
- `BeginScope<TState>` 返回 `null`，即**不支持日志作用域**（`using (_logger.BeginScope(...))` 不产生上下文分组效果）。