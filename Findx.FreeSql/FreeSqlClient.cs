using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Findx.FreeSql
{
    public class FreeSqlClient : IFreeSqlClient, IDisposable
    {
        private readonly ConcurrentDictionary<string, IFreeSql> pairs = new ConcurrentDictionary<string, IFreeSql>();
        private readonly FreeSqlOptions _options;

        public FreeSqlClient(FreeSqlOptions options)
        {
            _options = options.Value;
        }


        public IFreeSql Get(string primary = null)
        {
            primary = primary ?? _options.Primary;

            Check.NotNull(primary, nameof(primary));

            pairs.TryGetValue(primary, out var freesql);

            Check.NotNull(freesql, nameof(freesql));

            return freesql;
        }

        public bool Add(string primary, IFreeSql freeSql)
        {
            pairs[primary] = freeSql;
            return true;
        }

        public void Dispose()
        {
            pairs?.Clear();
        }

        public ICollection<IFreeSql> All()
        {
            return pairs.Values;
        }
    }
}
