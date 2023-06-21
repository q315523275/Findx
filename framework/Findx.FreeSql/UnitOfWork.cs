using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Findx.Data;
using Findx.Extensions;
using FreeSql.Internal.ObjectPool;

namespace Findx.FreeSql
{
    /// <summary>
    ///     FreeSql工作单元
    /// </summary>
    public class UnitOfWork : UnitOfWorkBase
    {
        private readonly IFreeSql _fsql;
        private Object<DbConnection> _conn;
        
        public UnitOfWork(IServiceProvider serviceProvider, IFreeSql fsql) : base(serviceProvider)
        {
            _fsql = fsql;
        }
        
        /// <summary>
        ///     归还数据库连接
        /// </summary>
        private void ReturnObject()
        {
            if (_conn != null)
            {
                _fsql.Ado.MasterPool.Return(_conn);
                _conn = null;
            }
        
            if (Transaction != null) Transaction = null;
        }

        /// <summary>
        /// 真实开启事物
        /// </summary>
        /// <param name="cancellationToken"></param>
        protected override async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            // 如果已获取数据库连接，先归还再获取
            if (_conn != null)
                _fsql.Ado.MasterPool.Return(_conn);
            
            // 数据库连接池获取连接
            _conn = _fsql.Ado.MasterPool.Get();
            
            try
            {
                Transaction = await _conn.Value.BeginTransactionAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                ReturnObject();
                // 抛出原始异常
                exception.ReThrow();
            }
        }

        protected override Task InternalSaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 当前不支持
            return Task.CompletedTask;
        }

        protected override async Task InternalCommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (Transaction?.Connection != null)
                {
                    await Transaction.CommitAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                ex.ReThrow();
            }
            finally
            {
                ReturnObject();
            }
        }

        protected override async Task InternalRollbackAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (Transaction?.Connection != null)
                {
                    await Transaction.RollbackAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                ex.ReThrow();
            }
            finally
            {
                ReturnObject();
            }
        }
    }
}