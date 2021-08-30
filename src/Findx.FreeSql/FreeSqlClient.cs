using System;
using System.Collections.Concurrent;

namespace Findx.FreeSql
{
    public class FreeSqlClient : ConcurrentDictionary<string, IFreeSql>, IDisposable
    {
        public void Dispose() => this.Clear();
    }
}
