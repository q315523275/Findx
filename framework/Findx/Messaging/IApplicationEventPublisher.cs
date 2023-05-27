using System.Threading.Tasks;

namespace Findx.Messaging;

/// <summary>
///     应用事件推送器(异步事件)
/// </summary>
public interface IApplicationEventPublisher
{
    /// <summary>
    ///     推送异步执行事件
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="applicationEvent"></param>
    /// <returns></returns>
    bool Publish<TEvent>(TEvent applicationEvent) where TEvent : IApplicationEvent;

    /// <summary>
    ///     推送异步执行事件
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="applicationEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync<TEvent>(TEvent applicationEvent, CancellationToken cancellationToken = default)
        where TEvent : IApplicationEvent;
}