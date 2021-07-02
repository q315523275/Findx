using Findx.DependencyInjection;
using Findx.EventBus.Abstractions;
using Findx.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Findx.WebHost.EventBus
{
    [Dependency(ServiceLifetime.Transient, TryRegister = true, AddSelf = true, ReplaceServices = true)]
    public class FindxTestEventHander : IEventHandler<FindxTestEvent>
    {
        public Task HandleAsync(FindxTestEvent @event)
        {
            Console.WriteLine($"EventHander:{@event.ToJson()}");
            return Task.CompletedTask;
        }
    }
}
