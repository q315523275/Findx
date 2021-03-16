using Findx.DependencyInjection;
using Findx.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebHost.Messaging
{
    public class CancelOrderCommandHandler : IApplicationListener<CancelOrderCommand, bool>, ITransientDependency
    {


        public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            Console.WriteLine(request.OrderId + "==========" + DateTime.Now.ToString());
            return request.OrderId > 0;
        }
    }
}
