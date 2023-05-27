using System;
using System.Collections.Generic;

namespace Findx.FreeSql
{
    /// <summary>
    ///     FreeSql
    /// </summary>
    public class FreeSqlClient : Dictionary<string, IFreeSql>, IDisposable
    {
        public void Dispose()
        {
            Clear();
        }
    }
}