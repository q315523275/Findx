using Findx.DependencyInjection;
using Findx.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebHost.Messaging
{
    public class CancelOrderCommandHandler : IMessageHandler<CancelOrderCommand, bool>, ITransientDependency
    {
        public CancelOrderCommandHandler()
        {
        }

        public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return request.OrderId > 0;
        }
    }
}
