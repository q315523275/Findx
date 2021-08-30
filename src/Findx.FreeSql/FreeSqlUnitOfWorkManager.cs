using Findx.Data;
using Findx.DependencyInjection;
using Findx.Extensions;
namespace Findx.FreeSql
{
    public class FreeSqlUnitOfWorkManager : UnitOfWorkManagerBase
    {
        private readonly FreeSqlClient _clients;
        public FreeSqlUnitOfWorkManager(FreeSqlClient clients, ScopedDictionary scopedDictionary) : base(scopedDictionary)
        {
            _clients = clients;
        }

        public override IUnitOfWork CreateConnUnitOfWork(string dbPrimary)
        {
            _clients.TryGetValue(dbPrimary, out var provider);

            Check.NotNull(provider, nameof(provider));

            return new FreeSqlUnitOfWork(provider);
        }
    }
}
