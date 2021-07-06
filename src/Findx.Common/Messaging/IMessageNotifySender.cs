using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    /// <summary>
    /// 通知消息推送器
    /// </summary>
    public interface IMessageNotifySender
    {
        /// <summary>
        /// 推送通知消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessageNotify;
    }
}
