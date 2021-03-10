using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    public interface IAsyncApplicationListener<in TMessage> where TMessage : IAsyncApplicationEvent
    {
        Task Handle(TMessage message, CancellationToken cancellationToken);
    }
}
