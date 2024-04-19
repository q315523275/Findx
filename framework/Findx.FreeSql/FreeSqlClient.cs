using System;
using System.Collections.Generic;

namespace Findx.FreeSql
{
    /// <summary>
    ///     FreeSql
    /// </summary>
    public class FreeSqlClient : Dictionary<string, IFreeSql>, IDisposable
    {
        private readonly FreeSqlOptions _options;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="options"></param>
        public FreeSqlClient(FreeSqlOptions options)
        {
            _options = options;
        }

        /// <summary>
        ///     获取默认
        /// </summary>
        /// <returns></returns>
        public IFreeSql GetDefault()
        {
            return this[_options.Primary];
        }
        
        public void Dispose()
        {
            Clear();
        }
    }
}