using Findx.Data;
using SqlSugar;
using System;
using System.Data.Common;

namespace Findx.SqlSugar
{
    public class SqlSugarUnitOfWork : IUnitOfWork<SqlSugarProvider>, IDisposable
    {
        private readonly SqlSugarProvider _provider;

        public SqlSugarUnitOfWork(SqlSugarProvider provider)
        {
            Check.NotNull(provider, nameof(provider));

            _provider = provider;
        }

        public bool HasCommitted { get; }
        public void EnableTransaction()
        {
            
        }

        public bool IsEnabledTransaction { get; }
        public DbConnection Connection { get { return (DbConnection)_provider.Ado.Connection; } }

        public DbTransaction Transaction
        {
            get
            {
                return (DbTransaction)_provider.Ado.Transaction;
            }
            set
            {
                _provider.Ado.Transaction = value;
            }
        }

        public void BeginOrUseTransaction()
        {
            _provider.Ado.BeginTran();
        }

        public void Commit()
        {
            _provider.Ado.CommitTran();
        }

        public void Rollback()
        {
            _provider.Ado.RollbackTran();
        }

        public SqlSugarProvider GetInstance()
        {
            return _provider;
        }

        public void Dispose()
        {
            if (Transaction != null)
                Transaction = null;
        }
    }
}
