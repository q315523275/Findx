namespace Findx.EventBus
{
    /// <summary>
    /// 事件处理上下文
    /// </summary>
    public class ProcessingContext
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="transportMessage"></param>
        /// <param name="eventObject"></param>
        public ProcessingContext(TransportMessage transportMessage, object eventObject = null)
        {
            Check.NotNull(transportMessage, nameof(transportMessage));

            TransportMessage = transportMessage;
            EventObject = eventObject;
        }
        /// <summary>
        /// 事件消息
        /// </summary>
        public TransportMessage TransportMessage { get; }
        /// <summary>
        /// 事件对象
        /// </summary>
        public object EventObject { get; }
    }
}
