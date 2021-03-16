using Findx.DependencyInjection;
using Findx.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebHost.Messaging
{
    public class PayedOrderCommandHandler : IAsyncApplicationListener<PayedOrderCommand>, ITransientDependency
    {

        public async Task Handle(PayedOrderCommand message, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            Console.WriteLine(message.OrderId + "=====IAsyncApplicationListener=====" + DateTime.Now.ToString());
        }
    }
}
