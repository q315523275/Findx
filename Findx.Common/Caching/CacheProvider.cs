using System.Collections.Generic;
using System.Linq;
namespace Findx.Caching
{
    public class CacheProvider : ICacheProvider
    {
        private readonly IDictionary<string, ICache> _caches;

        public CacheProvider(IEnumerable<ICache> caches)
        {
            _caches = caches.ToDictionary(it => it.Name, it => it);
        }

        public ICache Get(string name = CacheType.DefaultMemory)
        {
            _caches.TryGetValue(name, out ICache cache);

            Check.NotNull(cache, nameof(cache));

            return cache;
        }
    }
}
