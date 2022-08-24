using Findx.Data;
using SqlSugar;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.SqlSugar
{
    public class SqlSugarUnitOfWork : IUnitOfWork
    {
        private readonly SqlSugarProvider _provider;

        public SqlSugarUnitOfWork(SqlSugarProvider provider)
        {
            Check.NotNull(provider, nameof(provider));

            _provider = provider;
        }

        public IServiceProvider ServiceProvider { get; }
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

        public Task BeginOrUseTransactionAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (Transaction != null)
                Transaction = null;
        }
    }
}
