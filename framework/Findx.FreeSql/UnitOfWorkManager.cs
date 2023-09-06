using System;
using Findx.Common;
using Findx.Data;
using Findx.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.FreeSql
{
    public class UnitOfWorkManager : UnitOfWorkManagerBase
    {
        private readonly FreeSqlClient _clients;
        private readonly IOptionsMonitor<FreeSqlOptions> _options;
        private readonly IServiceProvider _serviceProvider;

        public UnitOfWorkManager(IServiceProvider serviceProvider, ScopedDictionary scopedDictionary) : base(
            scopedDictionary)
        {
            _serviceProvider = serviceProvider;
            _clients = serviceProvider.GetRequiredService<FreeSqlClient>();
            _options = serviceProvider.GetRequiredService<IOptionsMonitor<FreeSqlOptions>>();
        }

        protected override IUnitOfWork CreateConnUnitOfWork(string dbPrimary)
        {
            dbPrimary ??= _options.CurrentValue.Primary;

            _clients.TryGetValue(dbPrimary, out var fsql);

            Check.NotNull(fsql, nameof(fsql));

            return new UnitOfWork(_serviceProvider, fsql);
        }
    }
}