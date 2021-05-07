using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    public delegate Task<TResponse> MessageHandlerDelegate<TResponse>();
    public interface IMessagePipeline<in TRequest, TResponse>
    {
        Task<TResponse> Handle(TRequest request, MessageHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
    }
}
