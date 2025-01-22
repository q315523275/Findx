
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
>使用

```js
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

