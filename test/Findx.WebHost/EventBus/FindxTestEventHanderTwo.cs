// using Findx.DependencyInjection;
// using Findx.EventBus;
// using Findx.Extensions;
// using Microsoft.Extensions.DependencyInjection;
// using System;
// using System.Threading.Tasks;
//
// namespace Findx.WebHost.EventBus
// {
//     [Dependency(ServiceLifetime.Transient, TryRegister = true, AddSelf = true, ReplaceServices = true)]
//     [EventConsumer("Findx.Consumer2")]
//     public class FindxTestEventHanderTwo : IEventHandler<FindxTestEvent>
//     {
//         public Task HandleAsync(FindxTestEvent @event)
//         {
//             Console.WriteLine($"EventHanderTwo:{@event.ToJson()}");
//             return Task.CompletedTask;
//         }
//     }
// }

