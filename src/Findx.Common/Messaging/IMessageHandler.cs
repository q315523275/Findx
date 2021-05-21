using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    public interface IMessageHandler<in TRequest, TResponse> where TRequest : IMessageRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
