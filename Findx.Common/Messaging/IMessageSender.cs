using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    public interface IMessageSender
    {
        Task<TResponse> SendAsync<TResponse>(IMessageRequest<TResponse> message, CancellationToken cancellationToken = default);
    }
}
