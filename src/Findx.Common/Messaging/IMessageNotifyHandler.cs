using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    /// <summary>
    /// 泛型通知消息处理器
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IMessageNotifyHandler<in TMessage> where TMessage : IMessageNotify
    {
        /// <summary>
        /// 处理泛型知消消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Handle(TMessage message, CancellationToken cancellationToken = default);
    }
}
