using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    /// <summary>
    /// 消息发送器
    /// </summary>
    public interface IMessageSender
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResponse> SendAsync<TResponse>(IMessageRequest<TResponse> message, CancellationToken cancellationToken = default);
    }
}
