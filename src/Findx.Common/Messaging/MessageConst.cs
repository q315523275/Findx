namespace Findx.Messaging
{
	/// <summary>
	/// 消息内部变量
	/// </summary>
	internal static class MessageConst
	{
        /// <summary>
        /// 消息订阅处理器集合
        /// </summary>
        public static readonly IDictionary<Type, object> RequestMessageHandlers = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// 命令处理器字典
        /// </summary>
        public static readonly IDictionary<Type, object> CommandHandlers = new ConcurrentDictionary<Type, object>();
        
        /// <summary>
        /// 应用事件处理器字典
        /// </summary>
        public static readonly IDictionary<Type, object> ApplicationEventHandlers = new ConcurrentDictionary<Type, object>();
    }
}

