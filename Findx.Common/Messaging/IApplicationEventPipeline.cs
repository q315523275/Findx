using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    public delegate Task<TResponse> ApplicationEventHandlerDelegate<TResponse>();
    public interface IApplicationEventPipeline<in TRequest, TResponse>
    {
        Task<TResponse> Handle(TRequest request, ApplicationEventHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
    }
}
