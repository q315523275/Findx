using System.Data.Common;
using System.Threading.Tasks;
using Findx.Events;

namespace Findx.Data;

/// <summary>
///     工作单元
/// </summary>
public interface IUnitOfWork: IAsyncDisposable
{
    /// <summary>
    ///     工作单元编号
    /// </summary>
    Guid Id { get; }

    /// <summary>
    ///     服务提供起
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    ///     获取 是否已提交
    /// </summary>
    bool HasCommitted { get; }

    /// <summary>
    ///     获取 是否启用事务
    /// </summary>
    bool IsEnabledTransaction { get; }

    /// <summary>
    ///     数据库连接
    /// </summary>
    DbConnection Connection { get; }

    /// <summary>
    ///     工作单元事务
    /// </summary>
    DbTransaction Transaction { get; set; }

    /// <summary>
    ///     启用事务，事务代码写在 UnitOfWork.EnableTransaction() 与 UnitOfWork.Commit() 之间
    /// </summary>
    void EnableTransaction();

    /// <summary>
    ///     对数据库连接开启事务
    /// </summary>
    /// <param name="cancellationToken">异步取消标记</param>
    /// <returns></returns>
    Task BeginOrUseTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     保存变更
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     异步提交当前上下文的事务更改
    /// </summary>
    /// <returns></returns>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     异步回滚所有事务
    /// </summary>
    /// <returns></returns>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     添加事件至工作单元缓冲区
    /// <param name="eventData">IEvent事件</param>
    /// <param name="transactionPhase">触发阶段</param>
    /// </summary>
    void AddEventToBuffer<T>(T eventData, TransactionPhase transactionPhase = TransactionPhase.AfterCommit) where T : IEvent;
}