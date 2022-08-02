using Findx.Data;
using Findx.DependencyInjection;
using SqlSugar;

namespace Findx.SqlSugar
{
    public class SugarUnitOfWorkManager : UnitOfWorkManagerBase
    {
        private readonly SqlSugarClient _sqlSugar;
        public SugarUnitOfWorkManager(SqlSugarClient sqlSugarClient, ScopedDictionary scopedDictionary) : base(scopedDictionary)
        {
            _sqlSugar = sqlSugarClient;
        }

        protected override IUnitOfWork CreateConnUnitOfWork(string dbPrimary)
        {
            var provider = _sqlSugar.GetConnection(dbPrimary);
            Check.NotNull(provider, nameof(provider));

            return new SqlSugarUnitOfWork(provider);
        }
    }
}
