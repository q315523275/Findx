using System.Collections.Concurrent;
namespace Findx.FreeSql
{
    public class FreeSqlClient : IFreeSqlClient
    {
        private readonly ConcurrentDictionary<string, IFreeSql> pairs = new ConcurrentDictionary<string, IFreeSql>();
        public IFreeSql Acquire(string primary)
        {
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
    }
}
