using System.Threading.Tasks;
using Findx.Events;

namespace Findx.Data;

/// <summary>
/// 工作单元事物调度者
/// </summary>
public interface IUnitOfWorkEventDispatcher
{
    /// <summary>
    ///     添加事件至工作单元缓冲区
    /// </summary>
    void AddEventToBuffer<TEvent>(TEvent eventData, TransactionPhase transactionPhase = TransactionPhase.AfterCommit) where TEvent : IEvent;

    /// <summary>
    ///     执行触发事件
    /// </summary>
    /// <param name="transactionPhase"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ProcessEventAsync(TransactionPhase transactionPhase, CancellationToken cancellationToken = default);

    /// <summary>
    ///     清除事件
    /// </summary>
    void ClearAllEvents();
}