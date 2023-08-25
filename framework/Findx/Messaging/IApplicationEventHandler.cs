using System.Threading.Tasks;
using Findx.Events;

namespace Findx.Messaging;

/// <summary>
///     应用事件处理器
/// </summary>
public interface IApplicationEventHandler<in TEvent> : IEventHandler where TEvent : IApplicationEvent
{
	/// <summary>
	///     处理泛型知消消息
	/// </summary>
	/// <param name="eventData"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
}