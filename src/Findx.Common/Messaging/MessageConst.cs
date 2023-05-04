namespace Findx.Messaging;

/// <summary>
///     消息内部变量
/// </summary>
internal static class MessageConst
{
	/// <summary>
	///     消息订阅处理器集合
	/// </summary>
	public static readonly ConcurrentDictionary<Type, object> RequestMessageHandlers = new();

	/// <summary>
	///     命令处理器字典
	/// </summary>
	public static readonly ConcurrentDictionary<Type, object> CommandHandlers = new();

	/// <summary>
	///     应用事件处理器字典
	/// </summary>
	public static readonly ConcurrentDictionary<Type, object> ApplicationEventHandlers = new();
}