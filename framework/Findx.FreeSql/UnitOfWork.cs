using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Findx.Data;
using Findx.Extensions;
using FreeSql.Internal.ObjectPool;

namespace Findx.FreeSql;

/// <summary>
///     FreeSql工作单元
/// </summary>
public class UnitOfWork : UnitOfWorkBase
{
    private readonly IFreeSql _fsql;
    private Object<DbConnection> _conn;
        
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="fsql"></param>
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
    ///     真实开启事物
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
        catch (Exception ex)
        {
            ReturnObject();
            ex.ReThrow();
        }
    }

    /// <summary>
    ///     真实开启事物
    /// </summary>
    /// <param name="isolationLevel"></param>
    /// <param name="cancellationToken"></param>
    protected override async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        // 如果已获取数据库连接，先归还再获取
        if (_conn != null)
            _fsql.Ado.MasterPool.Return(_conn);
            
        // 数据库连接池获取连接
        _conn = _fsql.Ado.MasterPool.Get();
        try
        { 
            Transaction = await _conn.Value.BeginTransactionAsync(isolationLevel, cancellationToken);
        }
        catch (Exception ex)
        {
            ReturnObject();
            ex.ReThrow();
        }
    }

    /// <summary>
    ///     保存变更数据
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override Task InternalSaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 当前不支持
        return Task.CompletedTask;
    }
    
    /// <summary>
    ///     事物提交
    /// </summary>
    /// <param name="cancellationToken"></param>
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

    /// <summary>
    ///     事物回滚
    /// </summary>
    /// <param name="cancellationToken"></param>
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