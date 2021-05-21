using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    public interface IMessageNotifyHandler<in TMessage> where TMessage : IMessageNotify
    {
        Task Handle(TMessage message, CancellationToken cancellationToken = default);
    }
}
