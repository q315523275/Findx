namespace Findx.Messaging
{
    /// <summary>
    /// 请求消息标记
    /// </summary>
    public interface IMessageRequest { }
    /// <summary>
    /// 泛型请求消息标记
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public interface IMessageRequest<out TResponse> : IMessageRequest { }
}
