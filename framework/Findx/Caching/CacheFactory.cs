using Findx.DependencyInjection;

namespace Findx.Caching;

/// <summary>
/// 缓存服务工厂
/// </summary>
public class CacheFactory: ServiceFactoryBase<ICache>, ICacheFactory;