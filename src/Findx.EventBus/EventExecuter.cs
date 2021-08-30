using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.EventBus
{
    public class EventExecuter : IEventExecuter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventSubscribeManager _subsManager;
        private readonly IEventSerializer _serializer;

        public EventExecuter(IServiceProvider serviceProvider, IEventSubscribeManager subsManager, IEventSerializer serializer)
        {
            _serviceProvider = serviceProvider;
            _subsManager = subsManager;
            _serializer = serializer;
        }

        public async Task ExecuteAsync(EventMediumMessage message, CancellationToken cancellationToken = default)
        {
            if (_subsManager.HasSubscribeForEvent(message.EventName))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(message.EventName);
                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.IsDynamic)
                        {
                            if (!(scope.ServiceProvider.GetRequiredService(subscription.HandlerType) is IDynamicEventHandler handler)) continue;

                            await Task.Yield();
                            await handler.HandleAsync(message.Content);
                        }
                        else
                        {
                            var eventType = _subsManager.GetEventTypeByName(message.EventName);
                            if (eventType == null) continue;
                            var handler = scope.ServiceProvider.GetRequiredService(subscription.HandlerType);
                            if (handler == null) continue;
                            var integrationEvent = _serializer.Deserialize(message.Content, eventType);
                            var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);

                            await Task.Yield();
                            await (Task)concreteType.GetMethod("HandleAsync").Invoke(handler, new object[] { integrationEvent });
                        }
                    }
                }
            }
        }
    }
}
