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
            if (Options.MergeTrans)
                _sqlSugarClient.BeginTran();
            else
                _sqlSugarClient.Ado.BeginTran();
        }

        public void BeginTran(IsolationLevel il)
        {
            if (Options.MergeTrans)
                _sqlSugarClient.BeginTran();
            else
                _sqlSugarClient.Ado.BeginTran(il);
        }

        public void CommitTran()
        {
            if (Options.MergeTrans)
                _sqlSugarClient.CommitTran();
            else
                _sqlSugarClient.Ado.CommitTran();
        }

        public SqlSugarClient GetInstance()
        {
            return _sqlSugarClient;
        }

        public void RollbackTran()
        {
            if (Options.MergeTrans)
                _sqlSugarClient.RollbackTran();
            else
                _sqlSugarClient.Ado.RollbackTran();
        }
    }
}
