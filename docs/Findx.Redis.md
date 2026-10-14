# Findx.Redis

Redis 组件，基于 StackExchange.Redis 实现公共缓存组件，实现 `IRedisClient` 统一操作功能，实现分布式锁功能，支持多实例操作、支持独立序列化配置。

> Nuget 包引用

```
Findx.Redis
```

目标框架：`net6.0` / `net8.0` / `net10.0`，底层依赖 `StackExchange.Redis`（net6.0 为 3.1.31，net8.0/net10.0 为 3.2.0）。

> 本包无独立的 DI 扩展方法

通过 `StartupModule` 机制自动装配：

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 RedisModule
```

`RedisModule`（`ModuleLevel.Framework`，`Order = 110`）注册：

```csharp
services.AddSingleton<IConnectionProvider, ConnectionProvider>();
services.AddSingleton<IRedisClientProvider, RedisClientProvider>();
services.AddSingleton(sp => sp.GetRequiredService<IRedisClientProvider>().CreateClient());  // IRedisClient
services.AddSingleton<IRedisSerializer, RedisSerializer>();
services.AddSingleton<ILock, RedisLock>();     // 替换核心包的 LocalCacheLock
services.AddSingleton<ICache, RedisCache>();   // 替换核心包的 InMemoryCache
```

> 重要

启用 Redis 会**同时替换 `ICache` 与 `ILock`**。此时 `ICacheFactory.Create(CacheType.DefaultMemory)` 仍能拿到内存缓存实现，但直接注入 `ICache` / `ILock` 拿到的是 Redis 实现。

## Json 配置

```json
{
    "Findx": {
        "Redis": {
            "Enabled": true,
            "Connections": {
                "Default": "127.0.0.1:6379,defaultDatabase=0,password=",
                "Log": "10.0.0.9:6379,defaultDatabase=1"
            }
        }
    }
}
```

> 配置说明

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `Enabled` | `false` | 是否启用 Redis。**必须显式写 `true`**，否则整个模块不注册 |
| `Connections` | 空字典 | 连接名 → 连接串，连接串直接传给 StackExchange.Redis |
| `Connections:Default` | 空 | 默认连接名，连接名未命中时回落到该项 |

> 注意事项

模块判定是否启用时读取的是 `IConfiguration` 原始值（`GetValue<bool>`），**缺省为 false**。虽然 `RedisOptions.Enabled` 属性的默认值是 `true`，但必须显式配置 `"Enabled": true` 才会生效。

## 客户端使用

`IRedisClient` 提供约 130 个方法，覆盖 Redis 全部常用数据类型，每个方法都有同步与异步两个版本。

> 分组一览

| 分组 | 主要方法 |
|---|---|
| Keys / 脚本 | `SearchKeys`、`Ttl`、`Exists`、`Expire`、`Remove`、`RemoveAll`、`Eval` |
| String | `StringSet`、`StringGet`、`IncrBy`、`IncrByFloat`、`DecrBy`、`DecrByFloat` |
| Hash | `HSet`、`HMSet`、`HGet`、`HMGet`、`HGetAll`、`HVals`、`HKeys`、`HLen`、`HExists`、`HDel`、`HIncrBy`、`HDecr`、`HIncrByFloat`、`HDecrByFloat` |
| List | `LPush`、`RPush`、`LPop`、`RPop`、`LRange`、`LRem`、`LSet`、`LTrim`、`LIndex`、`LLen`、`LInsertBefore`、`LInsertAfter` |
| Set | `SAdd`、`SCard`、`SIsMember`、`SMembers`、`SRandMembers`、`SPop`、`SRem` |
| SortedSet | `ZAdd`、`ZCard`、`ZCount`、`ZLexCount`、`ZIncrBy`、`ZRange`、`ZRangeByScore`、`ZRank`、`ZScore`、`ZRem`、`ZRemRangeByRank`、`ZRemRangeByScore` |
| Geo | `GeoAdd`、`GeoDist`、`GeoHash`、`GeoPos`、`GeoRadius` |
| HyperLogLog | `PfAdd`、`PfCount`、`PfMerge` |
| Lock | `Lock`、`LockRelease` |

> 默认连接

```csharp
[FromServices] IRedisClientProvider redisClientProvider

var redis = redisClientProvider.CreateClient();          // 默认连接

await redis.StringSetAsync("user:1", json, TimeSpan.FromMinutes(30));
var value = await redis.StringGetAsync("user:1");

await redis.HMSetAsync("user:1", new Dictionary<string, string>
{
    { "name", "Findx" }, { "age", "18" }
}, TimeSpan.FromHours(1));
var hash = await redis.HGetAllAsync("user:1");
```

> 多连接

```csharp
var logClient = redisClientProvider.CreateClient("Log");
```

连接实例由 `ConnectionProvider` 按连接名惰性创建并缓存（`ConcurrentDictionary<string, Lazy<ConnectionMultiplexer>>`），建连失败会移除缓存并重抛异常。

> 独立序列化配置

```csharp
var custom = redisClientProvider.CreateClient(new MyJsonRedisSerializer(), "Log");
```

`IRedisSerializer` 默认实现基于 System.Text.Json，可自定义（如 Protobuf、MessagePack）。

> Geo 示例

```csharp
await redis.GeoAddAsync("shops", new List<(double longitude, double latitude, string member)>
{
    (118.763709, 32.106839, "shop-1")
}, ct);

var near = await redis.GeoRadiusAsync("shops", "shop-1", 3000, "m", ct);
foreach (var (member, distance) in near)
{
    Console.WriteLine($"{member}: {distance}米");
}
```

`unit` 支持 `"m"`（默认）/ `"km"` / `"ft"` / `"mi"`。

> 注意事项

- `IRedisClient` **不含 Pub/Sub** 能力。
- `SearchKeys` 在大数据量下会阻塞 Redis，生产环境慎用。
- `HKeys` 没有异步版本；`ZRangeByScore` 的同步版 `count` 默认 `-1`，异步版默认 `0`，使用时需显式传参。
- `HDel` / `SRem` 传入空集合时会删除整个 key。

## 分布式锁

`RedisLock` 实现了核心包的 `ILock` 抽象，锁名 `DistributedLock.Redis`。

> 基础用法

```csharp
var locked = await redis.LockAsync("order:1001", "token-abc", TimeSpan.FromSeconds(30));
// 业务处理
var released = await redis.LockReleaseAsync("order:1001", "token-abc");   // value 必须与加锁一致
```

> 通过 ILockFactory 使用（推荐，支持续期）

```csharp
var @lock = _lockFactory.Create("DistributedLock.Redis");

using var rlock = await @lock.AcquireAsync("order:1001", TimeSpan.FromSeconds(30), isWait: true, renew: true);
if (!rlock.IsLocked()) return;    // 未拿到锁时返回 null

try
{
    // 临界区
}
finally
{
    await rlock.ReleaseAsync();
}
```

> 语法糖

```csharp
await @lock.ExecuteWithLockAsync("order:1001", TimeSpan.FromSeconds(30), async () =>
{
    // 临界区
});
```

> 实现要点

| 项 | 说明 |
|---|---|
| 锁 key | `lock:{resource}`，统一前缀避免与业务 key 冲突 |
| 锁 value | `Guid.NewGuid().ToString()`，作为持有者标识 |
| 加锁 | 底层 `LockTakeAsync`，value 校验与 TTL **一次性原子完成** |
| 释放 | 底层 `LockRelease`，Lua 校验 value 匹配才删除，**不会误删他人锁** |
| 续期 | 先比对 value 再续 TTL |

> 自动续期

`renew: true` 时 `RLock` 内部启动一个定时器，周期为**锁时长的 1/3**，每次续期递增 `RenewalCount`。

> 注意事项

- `RLock.ReleaseAsync` 有幂等保护，可重复调用。
- 分布式锁的安全性依赖「释放时校验 value」，调用方不要复用固定 value。

## 缓存实现

`RedisCache` 实现了核心包的 `ICache`，缓存名 `redis.default`（`CacheType.DefaultRedis`），使用 Hash 结构存储：值存 field 为 key 本身，滑动过期存 field 为 `"SlidingExpiration"`。

```csharp
var cache = _cacheFactory.Create(CacheType.DefaultRedis);
await cache.TryAddAsync("key", value, TimeSpan.FromMinutes(5));
var v = await cache.GetAsync<string>("key");
```

> 注意事项

- 核心包的 `ICache` 定义了滑动过期选项，Redis 端滑动过期**暂未实现**。
- `Ttl` 无过期时间时返回 `-1`。

## 使用示例

> 基础操作

```csharp
[HttpGet("exist")]
public async Task<string> KeyExist([FromServices] IRedisClientProvider provider)
{
    var redis = provider.CreateClient();
    await redis.ExpireAsync("exist", TimeSpan.FromSeconds(60));
    return (await redis.ExistsAsync("exist")).ToString();
}
```

> 自动续期锁

```csharp
[HttpGet("renewLock")]
public async Task<string> AutoLiveLock([FromServices] ILockFactory factory, CancellationToken ct)
{
    var @lock = factory.Create("DistributedLock.Redis");
    var rlock = await @lock.AcquireAsync("test_renew_lock", renew: true, cancellationToken: ct);
    if (!rlock.IsLocked()) return "未拿到锁";

    try
    {
        await Task.Delay(26 * 1000, ct);   // 超过默认 30s 锁时长，靠续期存活
    }
    finally
    {
        await rlock.ReleaseAsync();
    }

    return "ok";
}
```