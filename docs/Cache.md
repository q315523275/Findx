
## 缓存

缓存服务，框架实现本地缓存+Redis缓存，通过缓存工厂ICacheFactory来创建缓存实例接口。

缓存有效期包括持久、相对时间、绝对时间、滑动时间(Redis缓存端暂未实现)，系统内置缓存键组合生成器ICacheKeyGenerator，可用于根据多字符串值进行组合生成。

本地缓存InMemoryCache底层使用线程安全字典ConcurrentDictionary进行缓存服务实现。
> 缓存工厂接口

```js
/// <summary>
/// 缓存服务工厂
/// </summary>
public interface ICacheFactory: IServiceFactory<ICache>;
```
其中继承接口IServiceFactory专门用来处理一个服务多种实现的服务管理，对于此类型在Net8及以上可以直接使用IServiceProvider.GetService(typeof(xx), 服务名称)来直接管理。


> 缓存枚举


```js
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
public class OrderService
{
    private readonly ICache _cache;

    public OrderService(ICacheFactory cacheFactory)
    {
        _cache = cacheFactory.Create(CacheType.DefaultMemory);
    }

    public async Task<string> GetAsync(string orderNo, CancellationToken ct)
        => await _cache.GetOrAddAsync($"order:{orderNo}", _ => LoadOrderAsync(orderNo, ct), TimeSpan.FromMinutes(10));
}
```

## 实现要点与注意事项

| 关注点 | 行为 |
|---|---|
| 并发字典 | `InMemoryCache` 基于 `ConcurrentDictionary`，取值-写入统一走 `AddOrUpdate` 语义，避免并发下产生孤儿对象丢计数 |
| 前缀删除 | `RemoveByPrefix` 是**全量键扫描**（O(n)，逐个匹配前缀后删除），属运维级操作，不要放在请求热路径 |
| 滑动过期 | 本地缓存支持滑动过期；Redis 端**仅支持相对/绝对过期，滑动过期未实现** |
| TTL 语义 | Redis 端 `Ttl` 在键无过期时间时返回 `-1` |
| 键生成 | 内置 `ICacheKeyGenerator`，按多段字符串组合生成键，避免手写 `string.Format` 造成键风格不统一 |
| 多实现 | `ICacheFactory` 继承 `IServiceFactory<ICache>`，Net8+ 可直接 `GetService(typeof(ICache), 服务名)` 取指定实现 |

