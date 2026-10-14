# Findx.FreeSql

Orm-FreeSql 组件，实现 Findx 统一的仓储定义功能，扩展实现逻辑删除、多租户、多 FreeSql 实例、实体标注自动实现切库切表、工作单元、工作单元管理、工作单元事件、Sql 打印、慢 Sql 事件触发、分页总数是否查询、表集合及表字段查询、主键值自动创建等。

> Nuget 包引用

```
Findx.FreeSql
```

目标框架：`net6.0` / `net8.0` / `net10.0`，底层依赖 `FreeSql 3.5.311`。

> 本包无独立的 DI 扩展方法

本包不使用 `AddFreeSql(...)` 形式注册，而是通过 `StartupModule` 机制自动装配：

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 FreeSqlModule
```

`FreeSqlModule`（`ModuleLevel.Framework`，`Order = 50`）在 `ConfigureServices` 中按需注册：`FreeSqlClient`（多实例容器）、各数据源的 `IFreeSql`、仓储实现（Scoped）、工作单元管理器（Scoped）与审计实体报告（Singleton）。

> 装配内容

```csharp
services.AddSingleton(sp => new FreeSqlClient(FreeSqlOptions));

// 逐个数据源构建 IFreeSql，并按需注册共享别名
freeSqlClient.TryAdd(item.Key, fsql);
foreach (var sourceKey in item.Value.DataSourceSharing) freeSqlClient.TryAdd(sourceKey, fsql);

// 主库单独注册，可直接注入 IFreeSql
if (item.Key == FreeSqlOptions.Primary) services.AddSingleton(fsql);

// 替换核心包的抽象实现
services.Replace<IRepository<,>, RepositoryWithTypedId<,>>(ServiceLifetime.Scoped);
services.Replace<IRepository<>, Repository<>>(ServiceLifetime.Scoped);
services.Replace<IUnitOfWorkManager, UnitOfWorkManager>(ServiceLifetime.Scoped);
services.Replace<IAuditEntityReport, FreeSqlAuditEntityReport>(ServiceLifetime.Singleton);
```

> 未开启时整个模块不注册任何服务

## Json 配置

```json
{
    "Findx": {
        "FreeSql": {
            "Enabled": true,
            "Primary": "system",
            "Strict": true,
            "CheckInsert": true,
            "CheckUpdate": true,
            "DataSource": {
                "system": {
                    "ConnectionString": "",
                    "DbType": "MySql",
                    "DataSourceSharing": [],
                    "NameConvertType": "Underline",
                    "PrintSql": true,
                    "OutageDetection": true,
                    "OutageDetectionInterval": 1,
                    "SoftDeletable": true,
                    "MultiTenant": true,
                    "TenantFieldType": "String",
                    "MultiTenantFieldName": "TenantId",
                    "UseAutoSyncStructure": true,
                    "AuditEntity": true,
                    "AuditSqlRaw": true
                }
            }
        }
    }
}
```

> 配置说明

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `Enabled` | `true` | 是否启用模块 |
| `Primary` | 无 | 默认数据源名（`DataSource` 的键），只有该项对应的 `IFreeSql` 会被注册为可直接注入的实例 |
| `Strict` | `false` | 严格匹配数据源。`true` 时未匹配到实体指定数据源直接抛异常；`false` 时回退到 `Primary` |
| `CheckInsert` | `false` | 插入前自动赋值 `CreatedTime` / `CreatorId` / `Creator` |
| `CheckUpdate` | `false` | 更新前自动赋值 `LastUpdatedTime` / `LastUpdaterId` / `LastUpdater` |
| `ConnectionString` | 无 | 数据库连接字符串 |
| `DbType` | 无 | 数据库类型（`MySql` / `SqlServer` / `PostgreSql` / `Oracle` / `Sqlite`） |
| `DataSourceSharing` | 空 | 数据源共享，把这些名字也指向同一个 `IFreeSql` 实例 |
| `NameConvertType` | `None` | 字段名称转换类型，喂给 `UseNameConvert` |
| `PrintSql` | `false` | 打印 SQL 日志到 `ILogger<FreeSqlModule>` |
| `OutageDetection` | `false` | 开启慢 SQL 记录 |
| `OutageDetectionInterval` | `2` | 慢 SQL 阈值，**单位秒** |
| `SoftDeletable` | `false` | 开启逻辑删除全局过滤器 |
| `MultiTenant` | `false` | 开启多租户全局过滤器 |
| `TenantFieldType` | `String` | 租户字段类型（`String` / `Int` / `Long` / `Guid`） |
| `MultiTenantFieldName` | `TenantId` | 多租户字段名，插入时用于匹配列名自动赋值 |
| `UseAutoSyncStructure` | `false` | 自动同步实体结构，开发环境必备 |
| `AuditEntity` | `false` | 实体审计 |
| `AuditSqlRaw` | `false` | Sql 语句审计 |

> 注意事项

FreeSql **不会扫描程序集生成表**，只有实际执行 CRUD 时才会建表，因此开发环境建议开启 `UseAutoSyncStructure`。

## 多数据源与切库

> 实体上指定数据源

```csharp
[EntityExtension(DataSource = "system")]
public class SysUserInfo : EntityBase<long>, IFullAudited<long> { }

[EntityExtension(DataSource = "AuditLog")]
public class AuditLogInfo : EntityBase<long> { }
```

`DataSource` 的值对应 `Findx:FreeSql:DataSource` 中的键。开启 `Strict` 后，写错数据源名会在启动/首次访问时直接抛异常，便于及早发现配置错误。

> 数据源共享

```json
"DataSource": {
    "system": { "ConnectionString": "...", "DbType": "MySql" },
    "readSystem": { "ConnectionString": "", "DataSourceSharing": [ "system" ] }
}
```

共享的数据源名指向同一个 `IFreeSql` 实例，避免重复创建连接池。

> 直接使用 IFreeSql

```csharp
[HttpGet("tables")]
public object Tables([FromServices] IFreeSql fsql, string name)
    => fsql.DbFirst.GetTablesByDatabase(name).Select(x => new { x.Id, x.Name, x.Comment, x.Schema });

// 从仓储切换到原生查询
var sel = _repo.AsSelect().Where(x => x.Status == 1);
var val = _repo.CountIf(out long count);
```

`RepositoryExtensions` 提供 `AsFreeSql()`、`AsSelect()`、`AsUpdate()` 三个扩展，均已自动带上工作单元事务。

## 逻辑删除

开启 `SoftDeletable` 后会注册全局过滤器：

```csharp
fsql.GlobalFilter.ApplyOnly<ISoftDeletable>("SoftDeletable", it => it.IsDeleted == false);
```

- 查询自动过滤 `IsDeleted == true` 的数据。
- 仓储的 `Delete` / `DeleteAsync` 在实体实现了 `ISoftDeletable` 时自动改写为 `Update`，设置 `IsDeleted = true` 与 `DeletionTime`。
- 需要查询已删除数据时可使用 `AsSelect().DisableGlobalFilter("SoftDeletable")`。

## 多租户

开启 `MultiTenant` 并配置 `TenantFieldType` 后，按类型注册租户过滤器：

```csharp
fsql.GlobalFilter.ApplyOnlyIf<ITenant>("Tenant",
    () => TenantManager.Current.IsNotNullOrWhiteSpace(),
    it => it.TenantId == TenantManager.Current);
```

插入时若列为 `MultiTenantFieldName`（默认 `TenantId`）且值为空，自动填入 `TenantManager.Current`。

> 使用

```csharp
// 设置当前租户
TenantManager.Current = "000000000000000001";
TenantManager<long>.Current = 10001L;

// 通常由 FindxGlobalAttribute 从 ICurrentUser.TenantId 自动赋值
```

`TenantManager` 基于 `AsyncLocal<string>`，异步流自动隔离。

## 仓储

`IRepository<TEntity, TKey>` 的 FreeSql 实现，支持同步/异步双套方法。

> CRUD 示例

```csharp
private readonly IRepository<SysUserInfo, long> _repo;

// 分页
var page = await _repo.PagedAsync(1, 20,
    whereExpression: x => x.Name.Contains(name),
    sortConditions: [new SortCondition<SysUserInfo>("createdTime", ListSortDirection.Descending)]);

// 插入
await _repo.InsertAsync(new SysUserInfo { Name = "Findx" });

// 指定列更新
await _repo.UpdateAsync(model, updateColumns: x => new { x.Remark });

// 条件删除（支持逻辑删除）
await _repo.DeleteAsync(x => x.Status == 0);

// 字典更新（字典必须包含 Id 键，否则抛异常）
await _repo.UpdateColumnsAsync(new Dictionary<string, object> { { "id", id }, { "name", "123" } });

// 表信息
var tableName = _repo.GetDbTableName();
var columns = _repo.GetDbColumnName();
var dbType = _repo.GetDbType();
```

> 分表

```csharp
// 实体实现 ITableSharding 时自动分表
public class NewsInfo : EntityBase<long>, ITableSharding
{
    public string GetShardingTableName() => $"news_{DateTime.Now:yyyyMM}";
}

// 手动指定表名规则
var shardRepo = _repo.AsTable(() => $"news_{DateTime.Now:yyyyMM}");
```

> 注意事项

- `UpdateColumns` 传入的字典必须包含 `Id`（大小写不敏感）键，否则抛 `FindxException("505")`。
- `CountAsync` 内部会把结果先转 `int` 再隐式转 `long`，超大结果集场景需留意。

## 工作单元与事务

> 使用示例

```csharp
await using (var scope = ServiceLocator.CreateAsyncScope())
{
    var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

    // 扩展重载：enableTransaction 传 true 时会同时 EnableTransaction + BeginOrUseTransactionAsync
    await using (var uow = await uowManager.GetEntityUnitOfWorkAsync<TestNewsInfo>(true))
    {
        var repo = uow.GetRepository<TestNewsInfo, int>();
        await repo.InsertAsync(new TestNewsInfo { Id = 1, Title = "t" });

        // 缓冲提交后事件
        uow.AddEventToBuffer(new MyEvent(), TransactionPhase.AfterCommit);

        await uow.CommitAsync();
    }
    // DisposeAsync：未提交则自动回滚；已提交则派发 AfterCompletion 事件
}
```

> 指定数据源与隔离级别

```csharp
await using var uow = await uowManager.GetUnitOfWorkAsync("AuditLog", true, true, ct);
await uow.BeginOrUseTransactionAsync(IsolationLevel.ReadCommitted, ct);
```

> Web 端自动事务

```csharp
[Transactional]                                    // 或 [Transactional(DataSource = "AuditLog")]
public override Task<CommonResult> DeleteAsync(List<int> request, CancellationToken ct)
    => base.DeleteAsync(request, ct);
```

`TransactionalAttribute` 会按 `CommonResult.IsSuccess()` 或 HTTP 状态码决定提交还是回滚。

> 注意事项

- `EnableTransaction()` 必须**先于** `BeginOrUseTransactionAsync()`。
- 嵌套事务按计数实现，内层 `CommitAsync` 只弹出计数不真正提交。
- FreeSql 无变更跟踪，`UnitOfWork.InternalSaveChangesAsync` 是空实现，不影响 CRUD（CRUD 立即写库）。

## Sql 打印与慢 Sql 事件

> 打印

`PrintSql = true` 时通过 `ILogger<FreeSqlModule>` 输出：

```
Creating a new SqlSession
==>  Preparing:SELECT ...
==>  Parameters:2
==>  Column:Id  Row:1
==>  ExecuteTime:1.234ms
```

> 慢 Sql 事件

`OutageDetection = true` 且执行耗时超过 `OutageDetectionInterval` 秒时，发布 `SqlExecutionSlowEvent`：

```csharp
public class SqlExecutionSlowEvent : IApplicationEvent, IAsync
{
    public string SqlRaw { set; get; }
    public long ElapsedMilliseconds { set; get; }
}
```

> 订阅慢 Sql

```csharp
public class SlowSqlHandler : IApplicationEventHandler<SqlExecutionSlowEvent>, ITransientDependency
{
    public Task HandleAsync(SqlExecutionSlowEvent e, CancellationToken ct = default)
    {
        _logger.LogWarning($"慢Sql({e.ElapsedMilliseconds}ms): {e.SqlRaw}");
        return Task.CompletedTask;
    }
}
```

## 审计

开启 `AuditEntity` / `AuditSqlRaw` 后，本包通过 FreeSql 的 AOP 钩子采集实体变更与 SQL 执行信息，写入 `ScopedDictionary.AuditOperation`，最终由 `IAuditStore` 落库。

- `Aop.CurdBefore` 采集实体变更前的 `AuditEntityEntry`
- `Aop.AuditValue` 采集属性新值
- `Aop.CurdAfter` 采集 `AuditSqlRawEntry`（含 SQL、表名、耗时、参数）

> 注意事项

- 实体上的 `[EntityExtension(DisableAuditing = true)]` 可关闭该实体的审计。
- 需引入 `Findx.Extensions.AuditLogs` 或自行实现 `IAuditStore`，否则审计只记录不持久化。
- 不使用实体状态追踪，因此**无法获取实体属性变更之前的值**（`OriginalValue` 未赋值）。