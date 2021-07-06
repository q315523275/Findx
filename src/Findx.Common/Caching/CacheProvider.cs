using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
namespace Findx.Caching
{
    public class CacheProvider : ICacheProvider
    {
        private readonly IDictionary<string, ICache> _caches;
        private readonly string _defaultCache;
        public CacheProvider(IEnumerable<ICache> caches, IConfiguration configuration)
        {
            _caches = caches.ToDictionary(it => it.Name, it => it);
            _defaultCache = configuration.GetValue<string>("Findx:CacheType") ?? CacheType.DefaultMemory;
        }

        public ICache Get(string name = null)
        {
            name = name ?? _defaultCache;

            _caches.TryGetValue(name, out ICache cache);

            Check.NotNull(cache, nameof(cache));

            return cache;
        }
    }
}
