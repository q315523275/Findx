using Findx.Data;
using Findx.Extensions;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using Findx.Exceptions;
using Microsoft.Extensions.Logging;

namespace Findx.FreeSql
{
    /// <summary>
    /// FreeSql工作单元
    /// </summary>
    public class FreeSqlUnitOfWork : IUnitOfWork<IFreeSql>, IDisposable
    {
        private readonly ILogger<FreeSqlUnitOfWork> _logger;
        private readonly IFreeSql _fsql;
        private Object<DbConnection> _conn;
        
        private readonly Stack<string> _transactionStack = new Stack<string>();

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="fsql"></param>
        /// <param name="logger"></param>
        public FreeSqlUnitOfWork(IFreeSql fsql, ILogger<FreeSqlUnitOfWork> logger)
        {
            _fsql = fsql;
            _logger = logger;
            
            if (_fsql == null) 
                throw new ArgumentNullException(nameof(fsql));
        }
        
        /// <summary>
        /// 获取 是否已提交
        /// </summary>
        public bool HasCommitted { get; private set; }

        /// <summary>
        /// 获取 是否启用事务
        /// </summary>
        public bool IsEnabledTransaction => _transactionStack.Count > 0;

        /// <summary>
        /// 允许事务
        /// </summary>
        public void EnableTransaction()
        {
            var token = Guid.NewGuid().ToString();
            _transactionStack.Push(token);
            _logger.LogDebug($"允许事务提交，标识：{token}，当前总标识数：{_transactionStack.Count}");
        }

        public DbConnection Connection => _conn.Value;

        public DbTransaction Transaction { set; get; }
        
        /// <summary>
        /// 对数据库连接开启事务或应用现有同连接对象的上下文事务
        /// </summary>
        public void BeginOrUseTransaction()
        {
            if (!IsEnabledTransaction || Transaction != null)
                return;

            // 如果已获取数据库连接，先归还再获取
            if (_conn != null) 
                _fsql.Ado.MasterPool.Return(_conn);

            // 数据库连接池获取连接
            _conn = _fsql.Ado.MasterPool.Get();
            try
            {
                Transaction = _conn.Value.BeginTransaction();
            }
            catch(Exception exception)
            {
                ReturnObject();
                // 抛出原始异常
                exception.ReThrow();
            }
            
            HasCommitted = false;
        }

        /// <summary>
        /// 提交当前上下文的事务更改
        /// </summary>
        public void Commit()
        {
            if (HasCommitted || Transaction == null || Transaction.Connection == null)
            {
                return;
            }

            string token;
            if (_transactionStack.Count > 1)
            {
                token = _transactionStack.Pop();
                _logger.LogDebug($"跳过事务提交，标识：{token}，当前剩余标识数：{_transactionStack.Count}");
                return;
            }

            if (!IsEnabledTransaction)
            {
                throw new FindxException("500", "执行 IUnitOfWork.Commit() 之前，需要在事务开始时调用 IUnitOfWork.EnableTransaction()");
            }

            token = _transactionStack.Pop();
            
            try
            {
                if (Transaction?.Connection != null)
                {
                    Transaction.Commit();
                    _logger.LogDebug($"提交事务，标识：{token}，事务标识：{Transaction.GetHashCode()}");
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
            HasCommitted = true;
        }

        /// <summary>
        /// 回滚所有事务
        /// </summary>
        public void Rollback()
        {
            try
            {
                if (Transaction?.Connection != null)
                {
                    Transaction.Rollback(); 
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
            
            HasCommitted = true;
        }
        
        /// <summary>
        /// 归还数据库连接
        /// </summary>
        private void ReturnObject()
        {
            if (_conn != null)
            {
                _fsql.Ado.MasterPool.Return(_conn);
                _conn = null;
            }

            if (Transaction != null)
            {
                Transaction = null;
            }
        }

        /// <summary>
        /// 获取orm实例
        /// </summary>
        /// <returns></returns>
        public IFreeSql GetInstance()
        {
            return _fsql;
        }


        private int _disposeCounter;
        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            try
            {
                this.Rollback();
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}
