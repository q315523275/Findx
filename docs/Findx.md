# Findx

框架的核心组件包，几乎所有其他组件包都依赖它。该包只提供**核心接口定义**与**默认实现**，具体的第三方实现（ORM、缓存、日志、图片等）由配套组件包替换。

> Nuget 包引用

```
Findx
```

目标框架：`net6.0` / `net8.0` / `net10.0`

## 模块化

框架采用模块化的方式进行开发，各个模块独立管理，系统启动时自动扫描所有模块、然后执行各个模块对应初始化方法等。模块需继承 `StartupModule` 类，模块包含 `ConfigureServices`、`UseModule`、`OnShutdown` 等主要方法。

模块加载顺序：`ModuleLevel` Framework > Application > Business，同级别使用 `Order` 排序号排序

> 模块依赖

系统启动加载模块时，会自动识别当前模块所依赖模块，然后自动载入依赖模块。

```csharp
/// <summary>
///     Findx-Consul服务发现模块
/// </summary>
[Description("Findx-Consul服务发现模块")]
[DependsOnModules([typeof(DiscoveryCoreModule)])]
public class ConsulDiscoveryModule : StartupModule
```

> 模块加载控制

通过 `IFindxBuilder` 系统构建器，可以控制模块的加载、排除、使用。

```csharp
builder.Services.AddFindx().AddModules();
app.UseFindx();
```

`AddModules(params Type[] exceptModuleTypes)` 的参数是**需要排除**的模块类型，不是包含列表。

> 模块需要使用IApplicationBuilder

可以引用 `Findx.AspNetCore` 包，然后继承包里的 `WebApplicationModuleBase` 类

```csharp
public abstract class WebApplicationModuleBase : StartupModule
{
    public virtual void UseModule(WebApplication app) => base.UseModule(app.ApplicationServices);
}
```

更详细的模块说明见 [Module.md](Module.md)。

## 依赖注入

框架提供基于标记接口/特质的约定式自动注册。

| 标记接口 | 生命周期 |
|---|---|
| `ITransientDependency` | 瞬时 |
| `IScopeDependency` | 作用域 |
| `ISingletonDependency` | 单例 |

> 自动注册规则

实现类若无上述标记接口，需要显式标注 `[Dependency]`：

```csharp
[Dependency(ServiceLifetime.Scoped, AddSelf = true, TryRegister = true)]
public class MyService : IMyService
{
}

[IgnoreDependency]                   // 排除某个接口不参与自动注册
public interface IMyService { }
```

- 一个实现类有多个接口时，除第一个接口外，其余接口会被注册为委托到第一个接口的工厂，保证**同一实现类在多个接口下拿到同一实例**。
- 接口上标注 `[MultipleDependency]` 时允许同一接口注册多个实现（走 `Add` 而非单例注册）。
- 服务别名通过 `IServiceNameAware.Name` 提供，配合 `IServiceFactory<T>` 使用。

> 服务工厂

```csharp
public interface IServiceFactory<out TService> where TService : class
{
    TService Create(string name);
}
```

> 服务定位器

```csharp
ServiceLocator.GetService<ICache>();
ServiceLocator.CreateAsyncScope();
```

`ServiceLocator` 依赖 `UseFindx()` 已执行，否则解析会抛异常。

## 应用上下文

`IApplicationContext` 以单例形式暴露当前应用的元信息（环境、配置、端口、IP、版本、根目录）。

```csharp
public interface IApplicationContext
{
    IHostEnvironment HostEnvironment { get; }
    IConfiguration AppSetting { get; }
    string ApplicationId { get; }
    string ApplicationName { get; }
    string Uris { get; }
    int Port { get; }
    string Version { get; }
    string InstanceIp { get; }
    string InternalIp { get; }
    string RootPath { get; }
    string MapPath(string virtualPath);   // 物理路径映射
    void StopApplication();
}
```

> Json配置

```json
{
    "Findx": {
        "Application": {
            "Id": "",
            "Name": "",
            "Port": 5000,
            "Version": "",
            "Uris": "http://*:5000",
            "InstanceIp": "",
            "InternalIp": "",
            "AvailablePort": false
        },
        "CorrelationId": {
            "HttpHeaderName": "X-Correlation-Id",
            "SetResponseHeader": true
        }
    }
}
```

`AvailablePort` 为 `true` 时会校验端口占用，被占用则自动重新查找可用端口。

## 缓存

缓存服务，框架实现本地缓存+Redis缓存，通过缓存工厂 `ICacheFactory` 来创建缓存实例接口。

缓存有效期包括持久、相对时间、绝对时间、滑动时间(Redis缓存端暂未实现)，系统内置缓存键组合生成器 `ICacheKeyGenerator`，可用于根据多字符串值进行组合生成。

本地缓存 `InMemoryCache` 底层使用线程安全字典 `ConcurrentDictionary` 进行缓存服务实现。

> 缓存工厂接口

```csharp
/// <summary>
/// 缓存服务工厂
/// </summary>
public interface ICacheFactory: IServiceFactory<ICache>;
```

其中继承接口 `IServiceFactory` 专门用来处理一个服务多种实现的服务管理，对于此类型在 Net8 及以上可以直接使用 `IServiceProvider.GetService(typeof(xx), 服务名称)` 来直接管理。

> 缓存枚举

```csharp
/// <summary>
///     缓存服务类型
/// </summary>
public static class CacheType
{
    /// <summary>
    ///     内存
    /// </summary>
    public const string DefaultMemory = "memoryCache";

    /// <summary>
    ///     Redis
    /// </summary>
    public const string DefaultRedis = "redis.default";
}
```

> 使用

```csharp
private readonly ICacheFactory _cacheFactory;
public xxx(ICacheFactory cacheFactory)
{
    _cacheFactory = cacheFactory
}
xx()
{
    var cache = _cacheFactory.Create(CacheType.DefaultMemory);
    cache.xxxxx.
}
```

> 本地缓存统计

`InMemoryCache` 额外提供 `Count`、`Calls`、`Reads`、`Writes`、`Hits`、`Misses` 统计信息与 `ResetStats()`，便于做缓存命中率监控。滑动过期仅在 `Get` 时刷新，`Exists` 不会续期。

更详细的缓存说明见 [Cache.md](Cache.md)。

## 锁

分布式/本地互斥锁抽象，支持续期（`RLock`）与自动续期定时器。

```csharp
public interface ILock
{
    Task<RLock> AcquireAsync(string resource, TimeSpan? timeUntilExpires = null,
                             bool isWait = false, bool renew = false,
                             CancellationToken cancellationToken = default);
    Task ReleaseAsync(string resource, string lockId, CancellationToken cancellationToken = default);
    Task RenewAsync(string resource, string lockId, TimeSpan timeUntilExpires, CancellationToken cancellationToken = default);
}
```

> 使用

```csharp
var @lock = _lockFactory.Create("LocalCacheLock");

using var rlock = await @lock.AcquireAsync("order:1001", TimeSpan.FromSeconds(30), isWait: true, renew: true);
if (!rlock.IsLocked()) return;     // 未拿到锁时返回 null，务必判空

try
{
    // 临界区
}
finally
{
    await rlock.ReleaseAsync();
}

// 或者使用语法糖
await @lock.ExecuteWithLockAsync("order:1001", TimeSpan.FromSeconds(30), async () =>
{
    // 临界区
});
```

| 锁名称 | 实现 | 说明 |
|---|---|---|
| `LocalCacheLock` | `LocalCacheLock` | 单进程锁，基于内存缓存，多实例无效 |
| `DistributedLock.Redis` | `RedisLock` | 分布式锁，需引入 `Findx.Redis` |

> 注意事项

- `AcquireAsync` 未拿到锁时返回 `null`（不抛异常），务必用 `IsLocked()` 判空。
- 默认锁时长 30 秒，自动续期间隔为锁时长的 1/3。
- 总等待时间上限约等于传入的 `timeUntilExpires`。

## Cron

`CronUtility`，内置调度任务中使用

> 使用示例

```csharp
/// <summary>
///     任务计数
/// </summary>
public void Increment(DateTimeOffset dateTimeOffset)
{
    TryCount++;
    IsRunning = false;
    LastRunTime = NextRunTime;
    if (IsSingle)
    {
        NextRunTime = null;
    }
    else if (FixedDelay > 0)
    {
        NextRunTime = dateTimeOffset.AddSeconds(FixedDelay);
    }
    else if (!string.IsNullOrWhiteSpace(CronExpress))
    {
        NextRunTime = CronUtility.GetNextOccurrence(CronExpress, dateTimeOffset);
    }
}
```

底层为内嵌的 `Findx.Cronos.CronExpression`，支持 5 位（分 时 日 月 周）与 6 位（含秒）标准 Cron 表达式、`@daily` 等宏、`L`、`#`、`W` 语法。`CronUtility.GetNextOccurrence` 返回**本地时间**，无下次执行时返回 `default(DateTime)`。

更详细的 Cron 说明见 [Cron.md](Cron.md)。

## 调度任务

内置**内存版**调度任务，作业可由 Cron 表达式或固定延迟触发。

```csharp
[Job(Cron = "*/3 * * * *", Name = "演示自动任务")]
public class TestJob : IJob
{
    public Task<JobResult> RunAsync(JobExecuteContext context, CancellationToken ct = default)
        => Task.FromResult(JobResult.Success);
}
```

> 代码调度

```csharp
private readonly IJobScheduler _scheduler;

var id1 = await _scheduler.EnqueueAsync<TestJob>(TimeSpan.FromSeconds(30));      // 延时执行
var id2 = await _scheduler.ScheduleAsync<TestJob>("*/5 * * * * *");             // Cron 调度
var id3 = await _scheduler.ScheduleAsync<TestJob>(TimeSpan.FromMinutes(10));      // 固定延迟

await _scheduler.PauseJob(id2);
await _scheduler.ResumeJob(id2);
await _scheduler.RemoveJob(id2);
```

> Json配置

```json
{
    "Findx": {
        "Jobs": {
            "Enabled": true,
            "ScheduleType": "Simple",
            "ScheduleDelay": 5,
            "MisfireStrategy": "DoNothing",
            "MaxFetchJobCount": 100,
            "ServerPoolSize": 1,
            "ClientPoolSize": 5
        }
    }
}
```

> 注意事项

- 模块**默认关闭**，必须显式配置 `Findx:Jobs:Enabled=true`。
- 内存版存储 `InMemoryJobStorage` **进程重启即丢失**，且多实例不共享。`ScheduleType.Distributed` 本包未实现，需要自行实现 `IJobStorage`。

## 事件总线与消息

框架提供统一的进程内事件总线 `IEventBus`，按事件类型自动路由到不同通道。

```csharp
await _eventBus.PublishAsync(new MyEvent(), cancellationToken);
```

> 事件路由规则

| 事件类型 | 行为 |
|---|---|
| `IApplicationEvent`（未实现 `IAsync`） | **同步**执行处理器 |
| `IApplicationEvent` + `IAsync` | 进入队列**异步**多线程消费，不阻塞发布方 |
| `IIntegrationEvent` | 需自行注册 `IDistributedEventBus`，否则**静默丢弃** |

> 定义事件与处理器

```csharp
// 队列异步事件
public class PayedOrderCommand : IApplicationEvent, IAsync
{
    public long OrderId { get; set; }
}

public class PayedOrderHandler : IApplicationEventHandler<PayedOrderCommand>, ITransientDependency
{
    public async Task HandleAsync(PayedOrderCommand m, CancellationToken ct = default)
    {
        await Task.Delay(3000, ct);
    }
}
```

> 进程消息（CQRS 请求-响应）

```csharp
var res = await _messageBroker.SendRequestAsync(new CancelOrderCommand(orderId), ct);
```

```csharp
public class CancelOrderCommand : IMessageRequest<string>
{
    public string OrderId { set; get; }
}

public class CancelOrderCommandHandler : IMessageHandler<CancelOrderCommand, string>, ITransientDependency
{
    public Task<string> HandleAsync(CancelOrderCommand request, CancellationToken ct = default)
        => Task.FromResult(request.OrderId);
}
```

`IMessageHandler<,>` 通过 `GetRequiredService` 解析（同一请求类型只能有一个处理器）；`IApplicationEventHandler<>` 使用 `GetServices`（可有多个）。消息支持 `IMessagePipeline<,>` 管道拦截器实现 AOP。

> Json配置

| 键 | 默认值 | 说明 |
|---|---|---|
| `Findx:MessageQueueCapacity` | `0` | 队列容量，`<=0` 为无界通道；`>0` 为有界通道（满时阻塞发布方） |
| `Findx:MessageHandlerMaxTaskCount` | `0` | 消费者并发数，`0` 时取 `ProcessorCount + 1` |

## 数据访问

框架定义**与 ORM 无关**的数据访问契约，仓储与工作单元的具体实现由 `Findx.FreeSql` 提供。

### 实体约定

实体无需继承基类，实现对应接口即可被识别。

| 接口 | 说明 |
|---|---|
| `IEntity<TKey>` | 实体基接口，声明 `Id` |
| `ISoftDeletable` | 逻辑删除（`IsDeleted`、`DeletionTime`） |
| `ICreationAudited<TUser>` / `IUpdateAudited<TUser>` / `IFullAudited<TUser>` | 创建/更新审计 |
| `ITenant` / `ITenant<TKey>` | 多租户 |
| `ITableSharding` | 分表 |
| `IExpirable` | 数据有效期 |
| `IDataOwner<TUser>` / `IDataDepartment<TOrg>` | 数据归属 |

也提供了 `EntityBase<TKey>`、`FullAuditedBase<TKey, TUserKey>`、`ExpirableBase<TKey>` 等可选基类。

> 实体特性

```csharp
[EntityExtension(DataSource = "system", DisableAuditing = false)]
public class TestUserInfo : ISoftDeletable, IEntity<int>
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public DateTime? DeletionTime { get; set; }
    public bool IsDeleted { get; set; }

    public object Clone() => this.MemberwiseClone();
}
```

`DataSource` 指定实体所在的数据源（对应 `Findx:FreeSql:DataSource` 的键），实现切库。

### 仓储

```csharp
public interface IRepository<TEntity, in TKey> where TEntity : class, IEntity<TKey>
{
    int Insert(TEntity entity);
    Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    int Delete(TKey key);
    Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default);

    int Update(TEntity entity, Expression<Func<TEntity, object>> updateColumns = null,
        Expression<Func<TEntity, object>> ignoreColumns = null, bool ignoreNullColumns = false);

    Task<PageResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize,
        Expression<Func<TEntity, bool>> whereExpression = null,
        IEnumerable<SortCondition<TEntity>> sortConditions = null, bool hasTotalRows = true);

    IRepository<TEntity, TKey> AsTable(Func<string, string> tableRule);   // 分表
    Dictionary<string, object[]> CompareState(TEntity newData);            // 变更比较
}
```

> 使用示例

```csharp
private readonly IRepository<SysFileInfo, long> _repo;

var page = await _repo.PagedAsync(pageNo, pageSize,
    whereExpression: x => x.Name.Contains(name),
    sortConditions: [new SortCondition<SysFileInfo>("createdTime", ListSortDirection.Descending)]);

await _repo.UpdateAsync(model, updateColumns: x => new { x.ExtraProperties });   // 指定列更新
```

> 注意事项

- `DeleteAsync(whereExpression = null)` **默认删除全部数据**，务必传条件。
- `IRepository<,>.UnitOfWork` 是可写属性，通过 `WithUnitOfWork(uow)` 或 `uow.GetRepository<>()` 赋值。
- 未引入 `Findx.FreeSql` 时核心包只提供接口，仓储解析会失败。

### 工作单元与事务

```csharp
await using var uow = await uowManager.GetEntityUnitOfWorkAsync<TestNewsInfo>(true);
var repo = uow.GetRepository<TestNewsInfo, int>();
await repo.InsertAsync(entity);
uow.AddEventToBuffer(new MyEvent(), TransactionPhase.AfterCommit);
await uow.CommitAsync();
```

| 阶段 | 说明 |
|---|---|
| `BeforeCommit` | 提交前 |
| `AfterCommit` | 提交成功后（`AddEventToBuffer` 默认阶段） |
| `AfterRollback` | 回滚后 |
| `AfterCompletion` | 完成后（`DisposeAsync` 时派发） |

> 注意事项

- `EnableTransaction()` 必须**先于** `BeginOrUseTransactionAsync()`。
- 嵌套事务按计数实现，内层 `CommitAsync` 只弹出计数不真正提交，只有最外层才实际提交。
- 未开启真实事务时（`Transaction == null`）`CommitAsync` 会**静默返回**。
- `DisposeAsync` 时若未提交会自动回滚。
- 核心包默认的 `NullUnitOfWorkManager` 所有方法抛 `NotImplementedException`，需引入 `Findx.FreeSql`。

## 审计日志

审计日志，也称为审计追踪或审计记录，是在计算机系统、网络设备、数据库或其他 IT 环境中自动或手动生成的一系列记录。

>框架实现层面主要记录一下信息

1. 请求人相关信息，如 用户编号、用户名、昵称、租户编号
2. 请求信息，如 方法、客户端 Ip、UserAgent、请求时间、返回时间、请求结果、异常信息
3. 报文信息，如 http.url、http.path、http.method、http.status_code、http.request.body、http.response.body
4. 执行 SQL 信息，如 执行的 SQL 语句、语句参数、执行耗时
5. 实体信息，如 实体路径、实例操作属性集合及属性值

>使用说明

系统通过 `AuditOperationAttribute` 来进行触发记录。可以将 `AuditOperationAttribute` 放在 Controller 类上或者 Action 上，也可以进行全局配置。当进行全局配置时，可以通过 `DisableAuditingAttribute` 来控制关闭某些接口的审计。

>使用示例

```csharp
// 全局配置
builder.Services.AddControllers().AddMvcFilter<AuditOperationAttribute>()

// Controller或者Action
[HttpGet("update"), AuditOperation]
public async Task<Guid> UpdateAsync()

// 禁用
[HttpGet("metrics"), Description("系统指标")]
[DisableAuditing]
public async Task<CommonResult> MetricsAsync()

// Json配置
{
    "Findx": {
        "Auditing": {
           "Enabled": true,          // 是否开启
           "ExtractRequestBody": false, // 是否提取请求报文
           "ExtractResponseBody": false // 是否提取返回报文
        }
    }
}
```

>存储扩展

- 由于存储需要使用存储介质，每个项目可能都不同，可以自实现 `IAuditStore` 里的 `SaveAsync` 方法。建议使用异步方式进行记录。
- 系统也内置了数据库的存储，可以查看扩展包 `Findx.Extensions.AuditLogs`

更详细的审计说明见 [Auditing.md](Auditing.md)。

## 统一返回结果

```csharp
public class CommonResult
{
    public string Code { set; get; }   // 成功 "0000"，失败 "500"
    public string Msg { set; get; }

    public static CommonResult Success();
    public static CommonResult<TData> Success<TData>(TData data);
    public static CommonResult Fail();
    public static CommonResult Fail(string code, string msg);
}

public class PageResult<TList>
{
    public int PageNo { get; }
    public int PageSize { get; }
    public long TotalRows { set; get; }
    public TList Rows { get; }
}
```

> 使用

```csharp
return CommonResult.Success(dto);
return CommonResult.Success<int>(0, "没有数据");
return CommonResult.Fail("4001", "参数错误");

if (result.IsSuccess()) { }
```

## 序列化

```csharp
public interface IJsonSerializer
{
    string Serialize(object obj, bool camelCase = false, bool indented = false);
    T Deserialize<T>(string json, bool camelCase = false);
}
```

默认实现基于 `System.Text.Json`，并提供常用类型转换器。

> 内置转换器

| 转换器 | 作用 |
|---|---|
| `DateTimeJsonConverter` / `DateTimeNullableJsonConverter` | 输出 `yyyy-MM-dd HH:mm:ss` |
| `LongStringJsonConverter` | `long` 序列化为字符串（避免前端 JS 精度丢失） |

> ASP.NET Core 管道配置

```csharp
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new LongStringJsonConverter());
});
```

> 默认约定

默认**忽略 null 值**、忽略只读字段、允许尾逗号与注释、数字可从字符串读取、循环引用被静默忽略。静态扩展 `ToJson()` / `ToObject<T>()` 绕过 DI 直接使用同一套配置。

## 文件存储

```csharp
public interface IFileStorage
{
    Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default);
    Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default);
    Task<bool> CopyFileAsync(string path, string targetPath, bool overwrite = false, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default);
}
```

多存储实现通过 `IStorageFactory.Create(name)` 创建，存储类型枚举为 `FileStorageType`（`Folder` / `Minio` / `Aliyun` / `Tencent`），核心包仅提供 `Folder`（本地目录）实现。

> Json配置

```json
{
    "Findx": {
        "Storage": {
            "Folder": {
                "DefaultFolder": "",
                "Host": "//127.0.0.1:5000"
            }
        }
    }
}
```

> 使用

```csharp
var storage = _storageFactory.Create(nameof(FileStorageType.Folder));
await storage.SaveFileAsync("storage/2026/10/14/abc.png", stream);
var text = await storage.GetFileContentsAsync("storage/config.json");
```

> 注意事项

- `path` 为相对于 `DefaultFolder` 的相对路径。
- 内部异常被捕获后返回 `false`，调用方无法区分失败原因。
- `DeleteFilesAsync` 传 `null` / `*` 会**递归删除整个根目录**，务必谨慎。

## 邮件与短信

```csharp
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, bool isBodyHtml = true, CancellationToken token = default);
    Task SendAsync(MailMessage message, bool normalize = true, CancellationToken token = default);
    Task SendAsync(IEnumerable<MailMessage> messages, bool normalize = true, CancellationToken token = default);
}
```

> Json配置

```json
{
    "Findx": {
        "Email": {
            "Host": "",
            "Port": 25,
            "UserName": "",
            "Password": "",
            "EnableSsl": false,
            "FromDisplayName": "",
            "FromAddress": ""
        }
    }
}
```

> 使用

```csharp
await _emailSender.SendAsync("a@b.com", "标题", "<h1>正文</h1>", isBodyHtml: true);
```

> 注意事项

- 默认实现基于 `System.Net.Mail.SmtpClient`，生产建议引入 `Findx.MailKit`。
- `ISmsSender` 默认实现 `NullSmsSender` **只打日志并静默成功**，需自行实现（仓库未内置短信组件）。
- `IEmailSender` 依赖 `IOptionsMonitor`，支持配置热更新。

## 图片处理与验证码

核心包只定义接口与参数 DTO，实现在 `Findx.ImageSharp`。

```csharp
public interface IImageProcessor
{
    Task<byte[]> ResizeAsync(byte[] imageByte, ImageResizeDto d, CancellationToken ct = default);
    Task<byte[]> CropAsync(byte[] imageByte, ImageCropDto d, CancellationToken ct = default);
    Task<byte[]> CompressAsync(byte[] imageByte, int quality = 75, CancellationToken ct = default);
    Task<byte[]> GrayAsync(byte[] imageByte, CancellationToken ct = default);
    Task<byte[]> BinaryAsync(byte[] imageByte, int threshold = 180, CancellationToken ct = default);
    Task<byte[]> RotateAsync(byte[] imageByte, float degrees, CancellationToken ct = default);
    Task<byte[]> FlipAsync(byte[] imageByte, ImageFlipMode mode, CancellationToken ct = default);
    Task<byte[]> MergeImageAsync(byte[] imageByte, byte[] mergeImageByte, MergeImageDto d, CancellationToken ct = default);
    Task<byte[]> DrawTextAsync(byte[] imageByte, string text, DrawTextDto d, CancellationToken ct = default);
    Task<byte[]> ImageWatermarkAsync(byte[] imageByte, byte[] watermarkImageByte, int location, float opacity, CancellationToken ct = default);
    Task<byte[]> LetterWatermarkAsync(byte[] imageByte, string text, int location, DrawTextDto d, CancellationToken ct = default);
}
```

> 使用

```csharp
var bytes = await _imageProcessor.ResizeAsync(source, new ImageResizeDto(800, 0, ImageResizeMode.Max));
```

## Html 转 Pdf

```csharp
public interface IPdfConverter
{
    Task<byte[]> ConvertAsync(string htmlString, CancellationToken cancellationToken = default);
}
```

> 使用

```csharp
var pdf = await _pdfConverter.ConvertAsync("<html><body><h1>报表</h1></body></html>", ct);
return File(pdf, "application/pdf");
```

实现在 `Findx.DinkToPdf`。

## Redis 接口

核心包定义与客户端库无关的 `IRedisClient` 契约（约 130 个方法，覆盖 Key/String/Hash/List/Set/SortedSet/Geo/HyperLogLog/锁/Lua），以及 `IRedisClientProvider` 与可插拔的 `IRedisSerializer`。

```csharp
public interface IRedisClientProvider
{
    IRedisClient CreateClient(string connectionName = null);
    IRedisClient CreateClient(IRedisSerializer redisSerializer, string connectionName = null);
}
```

> 使用

```csharp
var client = _redisClientProvider.CreateClient();          // 默认连接
var logClient = _redisClientProvider.CreateClient("Log");  // 指定连接名
await client.StringSetAsync("user:1", json, TimeSpan.FromMinutes(30));
```

> 注意事项

- 核心包**不注册** `IRedisClient`，需引入 `Findx.Redis`。
- `IRedisClient` **不含 Pub/Sub** 能力。
- 分布式锁方法 `Lock` / `LockRelease` 需调用方生成唯一 value，释放时必须与加锁值一致。

## Id 生成器

```csharp
public interface IKeyGenerator<out TKey> where TKey : IEquatable<TKey> { TKey Create(); }
public interface IGuidGenerator { Guid Create(); }
```

| 生成器 | 实现 | 说明 |
|---|---|---|
| `IKeyGenerator<long>` | `SnowflakeKeyGenerator` | 雪花 Id |
| `IKeyGenerator<Guid>` | `GuidKeyGenerator` | 委托 `IGuidGenerator` |
| `IGuidGenerator` | `SequentialGuidGenerator` | 有序 Guid（ABP 风格） |

> 使用

```csharp
var longId = _longKeyGenerator.Create();        // 雪花 Id
var guid = _guidKeyGenerator.Create();          // 有序 Guid

// 静态用法
long orderId = SnowflakeIdUtility.Default().NextId();
Guid g = SequentialGuidUtility.Next(SequentialGuidType.AsString);
```

> Json配置

```json
{
    "Findx": {
        "SequentialGuid": {
            "SequentialGuidType": "AsString"
        }
    }
}
```

> 注意事项

- `Guid` 与 `long` 主键在 JSON 序列化时建议转字符串输出。
- 需要更高性能的有序 Guid 可引入 `Findx.Guids.NewId`。

## 一致性哈希

`ConsistentHash`，主要用来解决分布式系统的数据分区问题；在服务发现的 IpHash 服务选择器中进行了使用。

> 使用示例

```csharp
// 一致哈希
var nodes = new ConsistentHash<string>();
nodes.Add("192.168.1.101");
nodes.Add("192.168.1.102");
nodes.Add("192.168.1.103");
nodes.Add("192.168.1.104");
nodes.Add("192.168.1.105");
nodes.Add("192.168.1.106");
var dict = new ConcurrentDictionary<string, int>();
for (var i = 0; i < 100000; i++)
{
    var node = nodes.GetItemNode("127.0.0.1"); // 指定固定内容
    dict.AddOrUpdate(node, 1, (_, value) => value + 1);
}
```

更详细的一致性哈希说明见 [ConsistentHash.md](ConsistentHash.md)。

## 拦截器

核心包定义与 DI 解耦的异步拦截器抽象，具体代理实现由 `Findx.Castle` 提供。

```csharp
public abstract class InterceptorBase : IInterceptor
{
    public abstract Task InterceptAsync(IMethodInvocation invocation);
}
```

`IMethodInvocation` 提供 `Arguments`、`ArgumentsDictionary`、`TargetObject`、`Method`、`ReturnValue`（可写）、`ProceedAsync()`。采用洋葱模型，需显式调用 `ProceedAsync()` 才会执行目标方法。

更详细的拦截器说明见 [Aspect.md](Aspect.md)。

## 对象映射

核心包定义 `IObjectMapper` 抽象与静态门面 `MapperExtensions`，实现在 `Findx.Mapster`。

```csharp
public interface IObjectMapper
{
    TTarget MapTo<TTarget>(object source);
    TTarget MapTo<TSource, TTarget>(TSource source, TTarget target);
}
```

> Json配置

```json
{
    "Findx": {
        "Mapping": {
            "IgnoreCase": true,
            "IgnoreNullValues": false
        }
    }
}
```

> 使用

```csharp
var dto = entity.MapTo<UserDto>();
```

> 注意事项

- 核心包**不注册** `IObjectMapper`，未引入 `Findx.Mapster` 时 `MapTo` 会抛异常。
- 标注 `[IgnoreMapping]` 的属性不参与映射。

## 异常处理

框架使用「异常通知者 + 多订阅器」模式。

```csharp
public class FindxException : Exception
{
    public FindxException(string code);
    public FindxException(string code, string message);
    public string ErrorCode { get; private set; }
    public string ErrorMessage { get; private set; }

    public static void ThrowIf(bool isThrow, string code, string msg);
}
```

> 自定义异常订阅

```csharp
public class DingTalkExceptionSubscriber : ExceptionSubscriber
{
    public override int Order => 1;    // 先于默认(10)执行

    public override async Task HandleAsync(ExceptionNotificationContext context, CancellationToken token = default)
    {
        if (context.Exception is ValidationException) return;   // 业务校验异常不告警
        // 发送告警
    }
}
```

`IExceptionSubscriber` 标注了 `[MultipleDependency]`，可注册多个实现。单个订阅器异常不会影响其它订阅器。

> 异常到 HTTP 响应的映射

由 `Findx.AspNetCore` 的 `UseJsonExceptionHandler()` 中间件完成。

## 日志

核心包提供 `StartupLogger`，解决「模块启动阶段 DI 与 Logger 尚未就绪」的问题。

```csharp
public class MyModule : StartupModule
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        var logger = services.GetOrAddSingletonInstance(() => new StartupLogger());
        logger.LogInformation($"模块初始化", nameof(MyModule));
        return services;
    }
}
```

启动日志在 `UseFindx()` 时统一输出到真正的 `ILogger`。日志 Provider 由 `Findx.NLog` 等组件包提供。

## 机器信息

跨平台读取系统与硬件信息，全部为静态方法，不经 DI。

```csharp
// 系统
SystemPlatformInfo.FrameworkDescription;    // .NET 版本
SystemPlatformInfo.OsDescription;
SystemPlatformInfo.ProcessorCount;
SystemPlatformInfo.MachineName;

// Cpu（必须两次采样求差值）
var v1 = CpuHelper.GetCpuTime();
await Task.Delay(1000);
var cpuValue = CpuHelper.CalculateCpuLoad(v1, CpuHelper.GetCpuTime());

// 内存
var memory = MemoryHelper.GetMemoryValue();
SizeInfo.Get((long)memory.TotalPhysicalMemory).ToString();     // 15.62Gb

// 网络
var network = NetworkInfo.TryGetRealNetworkInfo();
var oldRate = network.IpvSpeed();
await Task.Delay(1000);
var newRate = network.IpvSpeed();
var speed = NetworkInfo.GetSpeed(oldRate, newRate);            // 上传/下载速率

// 磁盘
foreach (var disk in DiskInfo.GetRealDisk())
{
    Console.WriteLine($"{disk.Id} {SizeInfo.Get(disk.FreeSpace)} / {SizeInfo.Get(disk.TotalSize)}");
}
```

> 注意事项

- 全部为同步阻塞调用，高频采集（如每次请求）有明显开销。
- Linux 容器内读取的是宿主机内存，数值会失真。

## IP 归属地

```csharp
public interface IIpGeolocation
{
    string GetLocationTextByIp(string ipAddress);
    IpGeolocationVo GetLocationByIp(string ipAddress);
    Dictionary<string, IpGeolocationVo> BatchGetLocation(IEnumerable<string> ipAddresses);
}
```

核心包默认注册的是 `NullIpGeolocation`（所有方法抛 `NotImplementedException`），实现在 `Findx.IP`。

## 规则引擎

```csharp
public interface IRulesEngineClient : IServiceNameAware
{
    Response Verify(string ruleRaw);
    Response RegisterRule(string ruleRaw);
    Task<Response> ExecuteAsync<TRequest>(string ruleName, TRequest data, CancellationToken cancellationToken = default);
    bool RuleExists(string ruleName);
    void RemoveRule(string ruleName);
    void ClearAllRules();
}
```

核心包只注册 `IRulesEngineFactory` 工厂，客户端实现在 `Findx.RulesEngine`。

> 注意事项

- 规则只存在于内存，应用重启丢失，需自行在启动时从数据库加载注册。
- `ExecuteAsync` 返回的是抽象 `Response`，业务结果从 `RuleResult.ActionResult.Output` 获取。

## 数据验证

基于 `DataAnnotations` 的扩展验证器集合，全部声明在 `System.ComponentModel.DataAnnotations` 命名空间下。

```csharp
public class CreateUserDto : ValidatableObject
{
    [Required, Chinese(ErrorMessage = "姓名必须是中文")]
    [StringLength(20, MinimumLength = 2)]
    public string Name { get; set; }

    [IdCard]
    public string IdCard { get; set; }

    [HttpUrlAddress]
    public string Homepage { get; set; }

    [CollectionSize(Min = 1, Max = 10)]
    public List<string> Tags { get; set; }

    [CustomAllowedValues("admin", "user", "guest")]
    public string Role { get; set; }
}
```

> 可用验证器

| 特性 | 说明 |
|---|---|
| `Chinese` | 必须是中文 |
| `IdCard` | 身份证号（仅校验长度与 X 尾位） |
| `HttpUrlAddress` | http/https URL |
| `CollectionSize` | 集合数量范围 |
| `CustomAllowedValues` | 白名单枚举值（忽略大小写） |

> 手动验证

```csharp
var results = dto.Validate();
if (results.Count > 0) throw new ValidationException(typeof(CreateUserDto), results);
```

> 注意事项

- 所有验证器对空值/空白返回 `Success`，必填需另加 `[Required]`。

## 表达式与工具集

### JSON 转表达式

把「JSON 描述的筛选/排序条件」转换为 LINQ `Expression`，用于动态查询与内存过滤。

```csharp
var filterGroup = new FilterGroup
{
    Logic = FilterOperate.And,
    Filters =
    [
        new FilterCondition { Field = "name", Value = "Findx", Operator = FilterOperate.Contains },
        new FilterCondition { Field = "Status", Value = "0,2", Operator = FilterOperate.In }
    ]
};

var filter = LambdaExpressionParser.ParseConditions<SysAppInfo>(filterGroup);
var list = entities.Where(filter.Compile());
```

支持的 `FilterOperate`：`Equal`、`NotEqual`、`Less`、`LessOrEqual`、`Greater`、`GreaterOrEqual`、`StartsWith`、`EndsWith`、`Contains`、`NotContains`、`In`、`NotIn`、`Between`，支持 `User.Name` 形式的嵌套字段。

> 注意事项

- `FilterGroup.Filters` 为空时会抛异常。
- `In` / `NotIn` 生成 `Enumerable.Contains` 调用，**无法下推 SQL**。

### 常用工具

```csharp
using Findx.Extensions;
using Findx.Utilities;

// 字符串
"abc".IsNullOrWhiteSpace();
"abc".ToCamelCase();
"abc".Truncate(10);

// 加密
EncryptUtility.Sha256("123456");
EncryptUtility.Md5By32("123456");
EncryptUtility.AesEncrypt("内容", key);

// 压缩
await CompressionUtility.CompressByBrotliAsync(bytes);
await CompressionUtility.CompressAsync("内容");

// 脱敏
DesensitizedUtility.ChineseName("张三");        // 张*
DesensitizedUtility.MobilePhone("13812345678"); // 138****5678

// 压缩与导出
await CsvUtility.ExportCsvAsync(list, path, rewrite: true);
var users = CsvUtility.ReadCsv<User>(path).ToList();

// 目录与文件
FileUtility.Copy(src, dst, true);
DirectoryUtility.Move(src, dst);

// 目录、文件、进程、网络、枚举、重试等
```

> 注意事项

- `CsvUtility` 不处理带引号字段，属性顺序须与列顺序一致，列数不匹配会静默丢行。
- `EncryptUtility` 存在默认 AES 密钥常量，生产环境务必显式传入密钥。
- `RandomUtility` 共享单个 `Random` 实例，高并发下非线程安全。
- `ProcessX.StartAsync(command)` 直接按空格切分命令，**不要把外部输入原样传入**，存在命令注入风险。

### 对象池

```csharp
using var sb = Pool.StringBuilder.Get(out var builder);
builder.Append("a");

var stream = Pool.MemoryStream.Rent();
```

### 反射类查询

```csharp
var types = AssemblyManager.FindTypesByBase<StartupModule>();
var attrTypes = AssemblyManager.FindTypesByAttribute<JobAttribute>();
```

> 注意事项

- `AssemblyManager.Filters` 按程序集名前缀过滤（默认含 `System`、`Microsoft`、`Window` 等），应用名以这些前缀开头会被误过滤。
- `AllTypes` / `AllAssemblies` 为一次性缓存，动态加载程序集后需手动 `Initialize()`。

### 进程命令执行

```csharp
var result = await ProcessX.StartAsync("dotnet --info").ToTask();     // string[]
var first  = await ProcessX.StartAsync("hostname").FirstAsync();      // 首行
byte[] bin = await ProcessX.StartReadBinaryAsync("cat", "/etc/hosts");
```

> 注意事项

- 退出码不在白名单时，异常在**枚举输出时**才抛出（`ProcessErrorException`），不消费输出则不会感知失败。
- `AcceptableExitCodes` 是全局静态可写属性。

## 组件实现对照

核心包提供抽象与默认实现，配套组件包提供第三方实现。安装组件包后由对应模块自动 `Replace`。

| 抽象 | 核心默认实现 | 组件包实现 |
|---|---|---|
| `IRepository<>` / `IUnitOfWorkManager` | `NullUnitOfWorkManager`（抛异常） | `Findx.FreeSql` |
| `ICache` | `InMemoryCache` | `Findx.Redis` → `RedisCache` |
| `ILock` | `LocalCacheLock` | `Findx.Redis` → `RedisLock` |
| `IRedisClient` | 无 | `Findx.Redis` |
| `IEmailSender` | `DefaultEmailSender`（SmtpClient） | `Findx.MailKit` |
| `ISmsSender` | `NullSmsSender`（只打日志） | 需自行实现 |
| `IImageProcessor` / `IVerifyCoder` | 无 | `Findx.ImageSharp` |
| `IPdfConverter` | 无 | `Findx.DinkToPdf` |
| `IObjectMapper` | 无 | `Findx.Mapster` |
| `IRulesEngineClient` | 无 | `Findx.RulesEngine` |
| `IIpGeolocation` | `NullIpGeolocation`（抛异常） | `Findx.IP` |
| `IInterceptor`（AOP 实现） | 无 | `Findx.Castle` |
| `IGuidGenerator` | `SequentialGuidGenerator` | `Findx.Guids.NewId` |
| `ILoggerProvider` | 不由核心提供 | `Findx.NLog` |
| `ICurrentUser` | `CurrentUser`（未注册） | `Findx.AspNetCore` |
| `IFileStorage`（非 Folder） | 无 | 需自行实现 |

> 需要自行注册（核心包未提供默认实现）

| 类型 | 说明 |
|---|---|
| `IAuditStore` | 审计落库实现，否则审计只记录不持久化 |
| `IEntityFinder` / `IDomainEventsDispatcher` | 领域事件派发，核心包中已被注释 |
| `IDistributedEventBus` | 分布式事件总线，未注册时分布式事件被静默丢弃 |
| `IMetricsClient` | 指标客户端，需手动注册 `DiagnosticsMetrics` |

## 相关文档

- [Module.md](Module.md) — 模块化详解
- [Aspect.md](Aspect.md) — 拦截器与 Castle 代理注册
- [Auditing.md](Auditing.md) — 审计日志详解
- [Cache.md](Cache.md) — 缓存详解
- [ConsistentHash.md](ConsistentHash.md) — 一致性哈希详解
- [Cron.md](Cron.md) — Cron 表达式说明