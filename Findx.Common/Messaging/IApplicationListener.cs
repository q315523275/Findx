using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    public interface IApplicationListener<in TRequest, TResponse> where TRequest : IApplicationEvent<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
