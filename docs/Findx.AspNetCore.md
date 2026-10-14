# Findx.AspNetCore

AspNetCore 组件包，提供 AspNetCore 的服务端功能封装，包含各个模块化构建、HttpClient 各策略创建、各个策略过滤器（防重、审计、异常、验证、租户、内网访问、限速、事物等）、全局异常处理、Crud、端点资源处理、Razor 视图读取器、Cors 配置、静态 `ServiceScopeResolver` 实现等。

> Nuget 包引用

```
Findx.AspNetCore
```

目标框架：`net6.0` / `net8.0` / `net10.0`，三个框架下公开 API 完全一致。

宿主适配实现通过 `AspNetCoreModule`（`ModuleLevel.Application`，`Order = 10`）装配：

```csharp
services.AddMemoryCache();
services.AddHttpContextAccessor();
services.TryAddScoped<IPrincipal>(p => p.GetService<IHttpContextAccessor>()?.HttpContext?.User);
services.TryAddScoped<ICurrentUser, CurrentUser>();
services.TryAddSingleton<IThreadCurrentClientIpAccessor, HttpContextClientIpAccessor>();
services.TryAddSingleton<IScopedServiceResolver, HttpContextServiceScopeResolver>();
services.Replace<ICancellationTokenProvider, HttpContextCancellationTokenProvider>(ServiceLifetime.Singleton);

// 关闭模型自动化验证，改由 FindxGlobalAttribute 自行输出验证结果
services.Configure<ApiBehaviorOptions>(opts => opts.SuppressModelStateInvalidFilter = true);
```

## MVC 过滤器

全部位于 `Findx.AspNetCore.Mvc.Filters` 命名空间。

| 过滤器 | 作用 | 关键属性（默认值） |
|---|---|---|
| `FindxGlobalAttribute` | 全局过滤器：赋值租户、模型验证失败返回 `400` | — |
| `ExceptionHandlerAttribute` | 异常处理，响应码固定 200 + `CommonResult` | — |
| `AuditOperationAttribute` | 操作审计，受 `Findx:Auditing` 控制 | — |
| `RateLimiterAttribute` | 限速，超限返回 `429` | `Key("")`、`Period("30s")`、`Limit(30)`、`IsDistributed(false)`、`Type(Ip)` |
| `AntiDuplicateRequestAttribute` | 防重提交，未拿到锁返回 `4019` | `Key("")`、`IsDistributed(false)`、`Interval("30s")`、`Type(User)` |
| `InternalNetworkLimiterAttribute` | 内网限定，非内网返回 `403` | — |
| `TransactionalAttribute` | 工作单元事务，按返回结果自动提交/回滚 | `DataSource(null)`、`EntityType(null)`、`IsolationLevel(null)` |
| `PreAuthorizeAttribute` | 预授权，追加权限资源标识 | `Authority` |

配套枚举：`RateLimitType { User, Ip, Global }`、`LockType { User, Ip, Global }`。

> 全局注册

```csharp
builder.Services.AddControllers()
    .AddMvcFilter<FindxGlobalAttribute>()
    .AddMvcFilter<ExceptionHandlerAttribute>()
    .AddMvcFilter<AuditOperationAttribute>()
    .AddMvcFilter<TransactionalAttribute>();
```

`AddMvcFilter<TFilter>` 有 `IMvcBuilder` 与 `IServiceCollection` 两个重载，都作用于**全局过滤器**集合。

> 局部使用

```csharp
[HttpGet("delete")]
[RateLimiter(Period = "10s", Limit = 5)]
[Transactional]
public async Task<CommonResult> DeleteAsync(List<int> request, CancellationToken ct) { }

[HttpPost("save")]
[PreAuthorize(Policy = FunctionRequirement.Policy, Roles = "admin", Authority = "Sys:User:Create")]
public async Task<CommonResult> SaveAsync(SaveDto dto, CancellationToken ct) { }
```

> 限速与防重的 Key 规则

```csharp
// 限速：RL:{客户端标识}{Key 或 请求路径}
return Key.IsNullOrWhiteSpace() ? $"RL:{clientId}{context.HttpContext.Request.Path}" : $"RL:{clientId}{Key}";

// 防重：ADR:{用户标识}{Key 或 请求路径}
return Key.IsNullOrWhiteSpace() ? $"ADR:{userId}{context.HttpContext.Request.Path}" : $"ADR:{userId}{Key}";
```

`IsDistributed = true` 时，限速使用 `CacheType.DefaultRedis`，防重使用 `ILock("DistributedLock.Redis")`；为 `false` 时使用内存缓存与本地锁。

> 注意事项

- `RateLimiterAttribute` 与 `AntiDuplicateRequestAttribute` 的 `[AttributeUsage]` **仅允许标注在方法上**；`InternalNetworkLimiterAttribute` 允许类与方法。
- `RateLimitType.Global` / `LockType.Global` 实现不完整，只有 `User` / `Ip` 分支被处理。
- 使用分布式计数时需自行保证计数器一致性。

## 中间件与管道扩展

> IApplicationBuilder / WebApplication 扩展

```csharp
app.UseCorrelationId()          // 关联 Id（X-Correlation-Id）
   .UseJsonExceptionHandler()   // 全局异常处理
   .UseCorsAccessor()           // CORS 策略 findx.cors
   .UseRouting();

app.UseFindx();                 // 执行各模块 UseModule
app.UseEndpointsWithAreaRoute();// Area 路由

app.UseFindxHosting();          // 追加 Uris 并 app.Run()
```

| 方法 | 说明 |
|---|---|
| `UseJsonExceptionHandler()` | JSON 异常处理中间件，返回统一 `CommonResult` |
| `UseCorrelationId()` | 生成/透传关联 Id，供日志链路追踪使用 |
| `UseCorsAccessor(policyName = "findx.cors")` | 启用 CORS |
| `UseEndpointsWithAreaRoute()` | 注册 Area 路由 + 默认路由 |
| `UseFindx()` | 遍历所有模块调用 `UseModule`，并注册停机回调 |
| `UseFindxStartup()` | 从 `IApplicationContext.Uris` 追加监听地址 |
| `UseFindxHosting()` | 追加监听地址并 `app.Run()`（终点，返回 `void`） |

> 异常响应映射

| 异常 | 响应 |
|---|---|
| `FindxException` | HTTP 200 + `CommonResult.Fail(ex.ErrorCode, ex.Message)` |
| `ValidationException` | HTTP 200 + `CommonResult.Fail("4001", ...)` |
| `BrokenCircuitException` | HTTP 200 + `CommonResult.Fail("500", "当前服务不可用,请稍后再试")` |
| `TimeoutRejectedException` | HTTP 200 + `CommonResult.Fail("500", "内部服务超时,请稍后再试")` |
| `HttpRequestException` | HTTP 200 + `CommonResult.Fail(状态码, "上游系统调用异常")` |
| 其他 `Exception` | **HTTP 500** + 纯文本 `error occurred`（便于外层组件熔断） |

> 管道顺序要求

1. `UseJsonExceptionHandler` / `UseCorrelationId` 尽量靠前，否则覆盖不到后续管道抛出的异常。
2. `UseCorsAccessor` 应在 `UseRouting()` 之前。
3. `UseFindx()` 必须在 `UseEndpointsWithAreaRoute()` 之前（各模块的 `UseModule` 会往管道里追加 `Use...`）。
4. `UseFindxHosting()` 是终点，其后代码不可达。

> 已废弃 API

| API | 替代 |
|---|---|
| `IEndpointRouteBuilder.MapControllersWithAreaRoute()` | `UseEndpointsWithAreaRoute()` |
| `IApplicationBuilder.UseFindx()`（空实现） | `WebApplication.UseFindx()` |
| `AspNetCoreModuleBase` | `WebApplicationModuleBase` |

## DI 扩展方法

```csharp
builder.Services.AddCorsAccessor();            // 注册 findx.cors 策略
builder.Services.AddRazorPageAndRenderer();    // AddRazorPages + IRazorViewRender
builder.Services.AddControllers().AddMvcFilter<TFilter>();
```

> Cors配置

```json
{
    "Findx": {
        "Cors": {
            "WithOrigins": [],
            "WithMethods": [],
            "WithHeaders": [],
            "AllowCredentials": false,
            "DisallowCredentials": false
        }
    }
}
```

`WithOrigins` / `WithMethods` / `WithHeaders` 为空时自动退化为 `AllowAnyOrigin()` / `AllowAnyMethod()` / `AllowAnyHeader()`。

> 注意事项

`AllowCredentials = true` 与 `AllowAnyOrigin()` 冲突，需显式配置 `WithOrigins`，否则运行时抛异常。

## HttpClient 策略

基于 Polly 的弹性策略扩展。

```csharp
builder.Services.AddHttpClient("policy")
                .AddFallbackPolicy(CommonResult.Fail(), 200)
                .AddCircuitBreakerPolicy(5, "5s")
                .AddRetryPolicy(1)
                .AddTimeoutPolicy(1);
```

| 方法 | 参数 | 说明 |
|---|---|---|
| `AddTimeoutPolicy` | `(int seconds)` | 超时策略，小于 1 时不添加 |
| `AddRetryPolicy` | `(int retryCount)` | 重试 5xx/408/429 及网络异常，小于 1 时不添加 |
| `AddCircuitBreakerPolicy` | `(int exceptionsAllowedBeforeBreaking, string durationOfBreak)` | 熔断，记录即将熔断/重置/半开启日志 |
| `AddFallbackPolicy` | `(object fallbackRspObj, int fallbackRspStatus)` | 降级为指定对象与状态码 |
| `AddFallbackPolicy` | `(Func<CancellationToken, Task<HttpResponseMessage>> fallbackAction)` | 降级为自定义响应 |

`durationOfBreak` 使用 `TimeSpanUtility.ToTimeSpan` 解析，仅支持 `ms` / `s` / `m` / `h` 后缀。

## Crud 与动态 API

提供一组约定式控制器基类，自动生成常用增删改查接口。

```
ControllerBase
  └─ ApiControllerBase                  [ApiController] [Route("api/[controller]/[action]")]
       └─ AreaApiControllerBase         [ApiController] [Route("api/[area]/[controller]/[action]")]
            └─ QueryControllerBase<TModel, TListVo, TDetailVo, TQueryParameter, TKey>
                 └─ CrudControllerBase<TModel, TListVo, TDetailVo, TCreateRequest, TUpdateRequest, TQueryParameter, TKey, TUserKey>
```

`QueryControllerBase` 自动生成：

| Action | 路由 | 说明 |
|---|---|---|
| `PageAsync` | `[HttpGet("page")]` | 分页列表 |
| `ListAsync` | `[HttpGet("list")]` | 全量列表 |
| `Detail` | `[HttpGet("detail")]` | 详情 |

可重写钩子：`CreateWhereExpression`、`CreateOrderExpression`、`ToListDto`、`ToDetailDto`、`DetailAfterAsync`。

`CrudControllerBase` 额外生成：

| Action | 路由 | 可重写钩子 |
|---|---|---|
| `AddAsync` | `[HttpPost("add")]` | `ToModelFromCreateRequest`、`AddBeforeAsync`、`AddAfterAsync` |
| `BatchAddAsync` | `[HttpPost("import")]` | `ImportBeforeAsync`、`ImportAfterAsync` |
| `EditAsync` | `[HttpPut("edit")]` | `ToModelFromUpdateRequest`、`EditBeforeAsync`、`EditAfterAsync` |
| `DeleteAsync` | `[HttpPost("delete")]` | `DeleteBeforeAsync`、`DeleteAfterAsync` |

> 使用示例

```csharp
[Route("api/[area]/[controller]")]
public class SysUserController : CrudControllerBase<
    SysUserInfo, SysUserDto, SysUserDto, SaveUserDto, SaveUserDto, PageUserDto, long>
{
    protected override Expression<Func<SysUserInfo, bool>> CreateWhereExpression(PageUserDto req)
        => x => x.Name.Contains(req.Name);

    protected override Task<CommonResult> DeleteAfterAsync(List<long> request, CancellationToken ct)
    {
        // 自定义删除后逻辑
        return base.DeleteAfterAsync(request, ct);
    }
}
```

> 注意事项

- `AddAsync` / `EditAsync` 内置实体审计赋值链（`CheckCreatedTime`、`CheckCreationAudited`、`CheckTenant`、`CheckOrg`、`CheckOwner`、`SetEmptyKey`）。
- 源码中 `ListAsync` 有硬编码 `if (req.PageSize == 20) req.PageSize = 2000;`（临时逻辑，带 TODO 注释），使用时需注意。

## 端点资源处理

启动时自动扫描所有 Controller Action，生成 `MvcFunction` 功能清单并缓存，供 `Findx.Security` 做接口级权限校验。

| 类 | 说明 |
|---|---|
| `MvcFunction` | 功能实体，继承 `FunctionBase`，含 `DisplayName`、`RouteTemplate` |
| `MvcFunctionHandler` | 遍历 `IActionDescriptorCollectionProvider` 提取功能信息 |
| `MvcFunctionStore` | 功能存储（**仅内存**，进程重启丢失） |
| `MvcFunctionWorker` | 启动时提取并同步功能信息 |

由 `MvcFunctionModule`（`ModuleLevel.Application`，`Order = 14`）装配。

> 属性到访问类型的映射

| 代码标注 | `FunctionAccessType` |
|---|---|
| 无标注 / `[AllowAnonymous]` | `Anonymous` |
| `[Authorize]` 无角色无资源 | `Login` |
| `[Authorize(Roles = "admin")]` | `RoleLimit` |
| `[PreAuthorize(Authority = "...")]` | `AuthorityLimit` |
| `[PreAuthorize(Roles = "...", Authority = "...")]` | `RoleAuthorityLimit` |

Action 未标注时**继承 Controller 上的标注**。

## Razor 视图读取器

```csharp
builder.Services.AddRazorPageAndRenderer();

var html = await _razorViewRender.RenderViewToStringAsync("~/Views/Report/Index.cshtml", model);
```

`RazorViewRender` 先尝试按视图名查找，失败后回退到当前 Action 的视图上下文，两者都失败时抛 `InvalidOperationException`。

## 请求与响应扩展

```csharp
context.GetClientIp();               // X-Forwarded-For[0] → REMOTE_ADDR → RemoteIpAddress → 127.0.0.1
request.GetUserAgentString();
request.GetUserAgent();              // 解析为 UserAgent 对象
request.IsAjaxRequest();
request.IsJsonContextType();
```

> 注意事项

`GetClientIp` 在所有来源都取不到 IP 时会抛异常，而限速、防重等过滤器每次请求都会调用它。

## 请求管道事件

三个事件均实现 `IApplicationEvent, IAsync`，通过 `IEventBus` 异步发布：

| 事件 | 属性 |
|---|---|
| `RateLimitedEvent` | `Key`、`Period`、`Limit`、`IsDistributed`、`Type`、`RequestedTimes`、`ClientIpAddress`、`UserAgent`、`TriggerTime` |
| `DuplicateRequestedEvent` | `LockType`、`LockKey`、`ClientIpAddress`、`UserAgent`、`TriggerTime` |
| `InternalNetworkLimitedEvent` | `ClientIpAddress`、`UserAgent`、`TriggerTime` |

可订阅这些事件做告警、风控埋点等。

## 启动示例

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.ReplacePlaceholders();

// 1. 框架核心 + 全部模块
builder.Services.AddFindx().AddModules();

// 2. CORS 策略 findx.cors
builder.Services.AddCorsAccessor();

// 3. HttpClient 弹性策略
builder.Services.AddHttpClient("policy")
                .AddFallbackPolicy(CommonResult.Fail(), 200)
                .AddCircuitBreakerPolicy(5, "5s")
                .AddRetryPolicy(1)
                .AddTimeoutPolicy(1);

// 4. MVC + 全局过滤器 + JSON 配置
builder.Services.AddControllers()
    .AddMvcFilter<FindxGlobalAttribute>()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new LongStringJsonConverter());
    });

var app = builder.Build();

// 5. 管道：追踪ID → JSON异常 → CORS → 路由 → 框架模块启动 → 端点
app.UseCorrelationId().UseJsonExceptionHandler().UseCorsAccessor();
app.UseRouting();
app.UseFindx();
app.UseEndpointsWithAreaRoute();

app.UseFindxHosting();
```