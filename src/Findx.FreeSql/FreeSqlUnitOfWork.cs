using Findx.Data;
using Findx.Extensions;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using Findx.Exceptions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Findx.FreeSql
{
    /// <summary>
    /// FreeSql工作单元
    /// </summary>
    public class FreeSqlUnitOfWork : IUnitOfWork
    {
        private readonly ILogger<FreeSqlUnitOfWork> _logger;
        private readonly IFreeSql _fsql;
        private Object<DbConnection> _conn;

        private readonly Stack<string> _transactionStack = new Stack<string>();

        private readonly ConcurrentBag<Func<IUnitOfWork, Task>> _completedHandlers = new ConcurrentBag<Func<IUnitOfWork, Task>>();

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="fsql"></param>
        /// <param name="logger"></param>
        public FreeSqlUnitOfWork(IServiceProvider serviceProvider, IFreeSql fsql, ILogger<FreeSqlUnitOfWork> logger)
        {
            ServiceProvider = serviceProvider;

            _fsql = fsql;
            _logger = logger;

            Check.NotNull(fsql, nameof(fsql));

            Id = Guid.NewGuid();
        }

        public bool HasCommitted { get; private set; }
        
        public bool IsEnabledTransaction => _transactionStack.Count > 0;

        public DbConnection Connection => _conn.Value;

        public DbTransaction Transaction { set; get; }

        public Guid Id { get; }
        
        public IServiceProvider ServiceProvider { get; }

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
        /// 允许使用事务
        /// </summary>
        public void EnableTransaction()
        {
            var token = Guid.NewGuid().ToString();
            _transactionStack.Push(token);
            _logger.LogDebug("允许事务提交，标识：{Token}，当前总标识数：{TransactionStackCount}", token, _transactionStack.Count);
        }
        
        /// <summary>
        /// 添加事物完成事件
        /// </summary>
        /// <param name="handler"></param>
        public void OnCompleted(Func<IUnitOfWork, Task> handler)
        {
            _completedHandlers.Add(handler);
        }

        #region Dispose

        /// <summary>
        /// 释放次数
        /// </summary>
        private int _disposeCounter;

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1)
                return;
            try
            {
                Rollback();

                _logger.LogDebug("工作单元生命周期结束，单元标识：{Id}，释放计量：{DisposeCounter}", Id, _disposeCounter);
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        #endregion
        
        #region 同步事物

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
                _logger.LogDebug("创建事务，事务标识：{HashCode}", Transaction.GetHashCode());
            }
            catch (Exception exception)
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
            if (HasCommitted || Transaction?.Connection == null)
            {
                return;
            }

            string token;
            if (_transactionStack.Count > 1)
            {
                token = _transactionStack.Pop();
                _logger.LogDebug("跳过事务提交，标识：{Token}，当前剩余标识数：{TransactionStackCount}", token, _transactionStack.Count);
                return;
            }

            if (!IsEnabledTransaction)
            {
                throw new FindxException("500",
                    "执行 IUnitOfWork.Commit() 之前，需要在事务开始时调用 IUnitOfWork.EnableTransaction()");
            }

            token = _transactionStack.Pop();

            try
            {
                if (Transaction?.Connection != null)
                {
                    Transaction.Commit();
                    _logger.LogDebug("提交事务，标识：{Token}，事务标识：{HashCode}", token, Transaction.GetHashCode());
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

                    _logger.LogDebug("回滚事务，事务标识：{HashCode}", Transaction.GetHashCode());
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

        #endregion

        #region 异步事物

        /// <summary>
        /// 开启事物异步
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task BeginOrUseTransactionAsync(CancellationToken cancellationToken = default)
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
                Transaction = await _conn.Value.BeginTransactionAsync(cancellationToken);
                _logger.LogDebug("创建事务，事务标识：{HashCode}", Transaction.GetHashCode());
            }
            catch (Exception exception)
            {
                ReturnObject();
                // 抛出原始异常
                exception.ReThrow();
            }

            HasCommitted = false;
        }

        /// <summary>
        /// 事物提交异步
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FindxException"></exception>
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (HasCommitted || Transaction?.Connection == null)
            {
                return;
            }

            string token;
            if (_transactionStack.Count > 1)
            {
                token = _transactionStack.Pop();
                _logger.LogDebug("跳过事务提交，标识：{Token}，当前剩余标识数：{TransactionStackCount}", token, _transactionStack.Count);
                return;
            }

            if (!IsEnabledTransaction)
            {
                throw new FindxException("500",
                    "执行 IUnitOfWork.Commit() 之前，需要在事务开始时调用 IUnitOfWork.EnableTransaction()");
            }

            token = _transactionStack.Pop();

            try
            {
                if (Transaction?.Connection != null)
                {
                    await Transaction.CommitAsync(cancellationToken);

                    _logger.LogDebug("提交事务，标识：{Token}，事务标识：{HashCode}", token, Transaction.GetHashCode());
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
        /// 事物回滚异步
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (Transaction?.Connection != null)
                {
                    await Transaction.RollbackAsync(cancellationToken);

                    _logger.LogDebug("回滚事务，事务标识：{HashCode}", Transaction.GetHashCode());
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

        #endregion
    }
}
