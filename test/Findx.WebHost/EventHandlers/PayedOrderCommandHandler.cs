using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Messaging;
using Findx.Utilities;

namespace Findx.WebHost.EventHandlers;

public class PayedOrderCommandHandler : IApplicationEventHandler<PayedOrderCommand>, ITransientDependency
{
    public Task HandleAsync(PayedOrderCommand message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"IMessageNotifyHandler<PayedOrderCommand>:{DateTime.Now}--{message.OrderId}");
        message.OrderId = SnowflakeIdUtility.Default().NextId();
        return Task.CompletedTask;
    }
}