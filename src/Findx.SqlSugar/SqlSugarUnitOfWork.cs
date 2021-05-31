using Findx.Data;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Data;

namespace Findx.SqlSugar
{
    public class SqlSugarUnitOfWork : IUnitOfWork<SqlSugarClient>
    {
        private readonly SqlSugarClient _sqlSugarClient;
        private readonly IOptionsMonitor<SqlSugarOptions> _options;
        public SqlSugarUnitOfWork(SqlSugarClient sqlSugarClient, IOptionsMonitor<SqlSugarOptions> options)
        {
            Check.NotNull(sqlSugarClient, nameof(sqlSugarClient));
            _sqlSugarClient = sqlSugarClient;
            _options = options;
        }
        private SqlSugarOptions Options
        {
            get
            {
                return _options?.CurrentValue;
            }
        }

        public void BeginTran()
        {
            _sqlSugarClient.Ado.BeginTran();
        }

        public void BeginTran(IsolationLevel il)
        {
            _sqlSugarClient.Ado.BeginTran(il);
        }

        public void CommitTran()
        {
            _sqlSugarClient.Ado.CommitTran();
        }

        public SqlSugarClient GetInstance()
        {
            return _sqlSugarClient;
        }

        public void RollbackTran()
        {
            _sqlSugarClient.Ado.RollbackTran();
        }
    }
}
