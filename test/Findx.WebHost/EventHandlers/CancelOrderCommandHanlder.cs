using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Messaging;

namespace Findx.WebHost.EventHandlers
{
    public class CancelOrderCommandHanlder : IMessageHandler<CancelOrderCommand, string>, ITransientDependency
    {
        public Task<string> HandleAsync(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.OrderId.ToString());
        }
    }
}

