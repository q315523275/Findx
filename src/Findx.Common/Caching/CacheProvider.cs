namespace Findx.Caching
{
    /// <summary>
    /// 缓存服务提供器
    /// </summary>
    public class CacheProvider : ICacheProvider
    {
        private readonly IDictionary<string, ICache> _caches;
        private readonly string _defaultCache;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="caches"></param>
        /// <param name="configuration"></param>
        public CacheProvider(IEnumerable<ICache> caches, IConfiguration configuration)
        {
            _caches = caches.ToDictionary(it => it.Name, it => it);
            _defaultCache = configuration.GetValue<string>("Findx:CacheType") ?? CacheType.DefaultMemory;
        }

        /// <summary>
        /// 获取缓存服务
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ICache Get(string name = null)
        {
            name = name ?? _defaultCache;

            _caches.TryGetValue(name, out ICache cache);

            Check.NotNull(cache, nameof(cache));

            return cache;
        }
    }
}
