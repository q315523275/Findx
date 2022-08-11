using Findx.Data;
using Findx.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Findx.FreeSql
{
    public class FreeSqlUnitOfWorkManager : UnitOfWorkManagerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly FreeSqlClient _clients;
        private readonly IOptionsMonitor<FreeSqlOptions> _options;

        public FreeSqlUnitOfWorkManager(IServiceProvider serviceProvider, ScopedDictionary scopedDictionary) : base(scopedDictionary)
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

            return new FreeSqlUnitOfWork(_serviceProvider, fsql, _serviceProvider.GetRequiredService<ILogger<FreeSqlUnitOfWork>>());
        }
    }
}
