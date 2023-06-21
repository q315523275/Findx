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
    void AddEventToBuffer<TEvent>(TEvent eventData) where TEvent : IEvent;
    
    /// <summary>
    /// 推送事件
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishEventsAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// 推送异步事件
    /// </summary>
    /// <returns></returns>
    Task PublishAsyncEventsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 清除事件
    /// </summary>
    void ClearAllEvents();
}