using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
namespace Findx.Caching
{
    public class CacheProvider : ICacheProvider
    {
        private readonly IDictionary<CacheType, ICache> _caches;
        private CachingOptions _options;

        public CacheProvider(IEnumerable<ICache> caches, IOptionsMonitor<CachingOptions> options)
        {
            _caches = caches.ToDictionary(it => it.Name, it => it);
            _options = options.CurrentValue;
            options.OnChange(ConfigurationOnChange);
        }

        private void ConfigurationOnChange(CachingOptions changeOptions)
        {
            if (changeOptions.ToString() != _options.ToString())
            {
                _options = changeOptions;
            }
        }

        public ICache Get(CacheType name = CacheType.Memory)
        {
            _caches.TryGetValue(name, out ICache cache);

            Check.NotNull(cache, nameof(cache));

            return cache;
        }
    }
}
