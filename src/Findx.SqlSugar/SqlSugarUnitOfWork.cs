using Findx.Data;
using SqlSugar;
using System.Data;

namespace Findx.SqlSugar
{
    public class SqlSugarUnitOfWork : IUnitOfWork<SqlSugarClient>
    {
        private readonly SqlSugarClient _sqlSugarClient;

        public SqlSugarUnitOfWork(SqlSugarClient sqlSugarClient)
        {
            Check.NotNull(sqlSugarClient, nameof(sqlSugarClient));
            _sqlSugarClient = sqlSugarClient;
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
