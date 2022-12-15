namespace Findx.WebSocketCore
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 文本内容
        /// </summary>
        Text,
        
        /// <summary>
        /// 连接事件
        /// </summary>
        ConnectionEvent,
        
        /// <summary>
        /// 异常
        /// </summary>
        Error
    }

    /// <summary>
    /// 消息内容模型
    /// </summary>
    public class WebSocketMessage
    {
        /// <summary>
        /// 类型
        /// </summary>
        public MessageType MessageType { get; set; }
        
        /// <summary>
        /// 内容
        /// </summary>
        public string Data { get; set; }
    }
    
    /// <summary>
    /// 泛型消息内容模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WebSocketMessage<T>
    {
        /// <summary>
        /// 类型
        /// </summary>
        public MessageType MessageType { get; set; }
        
        /// <summary>
        /// 内容
        /// </summary>
        public T Data { get; set; }
    }
}
