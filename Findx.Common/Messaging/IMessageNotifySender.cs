using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    public interface IMessageNotifySender
    {
        Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessageNotify;
    }
}
