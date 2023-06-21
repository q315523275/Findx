using System.Threading.Tasks;

namespace Findx.Events;

/// <summary>
/// 分布式事件执行器
/// </summary>
public interface IDistributedEventHandler<in TEvent>: IEventHandler where TEvent: IIntegrationEvent
{
    /// <summary>
    ///     处理泛型知消消息
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
}