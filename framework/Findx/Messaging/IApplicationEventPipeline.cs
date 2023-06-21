using Findx.Pipelines;

namespace Findx.Messaging;

/// <summary>
///     事件消息管理
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IApplicationEventPipeline<in TEvent>: IPipelineBehavior<TEvent>
{
}