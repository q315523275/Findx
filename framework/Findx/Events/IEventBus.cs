using System.Threading.Tasks;

namespace Findx.Events;

/// <summary>
///     事件总线
/// </summary>
public interface IEventBus
{
    /// <summary>
    ///     推送事件
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
}