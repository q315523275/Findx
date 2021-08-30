using System;
using System.Collections.Concurrent;

namespace Findx.DependencyInjection
{
    /// <summary>
    /// DI作用域字典(线程安全)
    /// </summary>
    public class ScopedDictionary : ConcurrentDictionary<string, object>, IDisposable
    {
        public void Dispose()
        {
            this.Clear();
        }
    }
}
