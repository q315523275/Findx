using Findx.Data;
using Findx.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.FreeSql
{
    public class FreeSqlUnitOfWorkManager : UnitOfWorkManagerBase
    {
        private readonly FreeSqlClient _clients;
        private readonly IOptionsMonitor<FreeSqlOptions> _options;
        public FreeSqlUnitOfWorkManager(FreeSqlClient clients, ScopedDictionary scopedDictionary, IOptionsMonitor<FreeSqlOptions> options) : base(scopedDictionary)
        {
            _clients = clients;
            _options = options;
        }

        protected override IUnitOfWork CreateConnUnitOfWork(string dbPrimary)
        {
            dbPrimary ??= _options.CurrentValue.Primary;
            
            _clients.TryGetValue(dbPrimary, out var fsql);

            Check.NotNull(fsql, nameof(fsql));

            return new FreeSqlUnitOfWork(fsql, ServiceLocator.GetService<ILogger<FreeSqlUnitOfWork>>());
        }
    }
}
