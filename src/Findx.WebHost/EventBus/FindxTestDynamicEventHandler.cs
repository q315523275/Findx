using Findx.DependencyInjection;
using Findx.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Findx.WebHost.EventBus
{
    [Dependency(ServiceLifetime.Transient, TryRegister = true, AddSelf = true, ReplaceServices = true)]
    public class FindxTestDynamicEventHandler : IDynamicEventHandler
    {
        public Task HandleAsync(string eventData)
        {
            Console.WriteLine($"DynamicEventHandler:{eventData}");
            return Task.CompletedTask;
        }
    }
}
