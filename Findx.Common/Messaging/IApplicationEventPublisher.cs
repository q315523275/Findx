using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    public interface IApplicationEventPublisher
    {
        Task<TResponse> SendAsync<TResponse>(IApplicationEvent<TResponse> message, CancellationToken cancellationToken = default);
        Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IAsyncApplicationEvent;
    }
}
