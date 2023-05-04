using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Messaging;

namespace Findx.WebHost.EventHandlers;

public class PayedOrderCommandHandler : IApplicationEventHandler<PayedOrderCommand>, ITransientDependency
{
    public async Task HandleAsync(PayedOrderCommand message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"IMessageNotifyHandler<PayedOrderCommand>:{DateTime.Now}");
        await Task.CompletedTask;
    }
}