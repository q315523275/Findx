using System.Threading.Tasks;

namespace Findx.Messaging
{
    /// <summary>
    /// 消息调度器(同步执行)
    /// </summary>
    public interface IMessageDispatcher
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResponse> SendAsync<TResponse>(IMessageRequest<TResponse> message, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 推送异步执行事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="applicationEvent"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task PublishAsync<TEvent>(TEvent applicationEvent, CancellationToken cancellationToken = default) where TEvent : IApplicationEvent;
    }
}
