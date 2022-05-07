using System;
using System.Collections.Generic;

namespace Findx.FreeSql
{
    public class FreeSqlClient : Dictionary<string, IFreeSql>, IDisposable
    {
        public void Dispose() => this.Clear();
    }
}
