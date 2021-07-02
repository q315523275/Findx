using Findx.EventBus.Abstractions;
using Findx.EventBus.Attributes;
using Findx.EventBus.Events;
using Findx.Extensions;
using Findx.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Findx.EventBus.RabbitMQ
{
    public class EventRabbitMqSubscriber : IEventSubscriber
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventRabbitMqSubscriber> _logger;
        private readonly IEventSubscriptionsManager _subsManager;
        private readonly IRabbitMqSerializer _serializer;
        private readonly IRabbitMqConsumerFactory _consumerFactory;
        private readonly IApplicationInstanceInfo _application;
        private readonly EventBusRabbitMqOptions _mqOptions;
        private ConcurrentDictionary<string, IRabbitMqConsumer> _consumers;

        public EventRabbitMqSubscriber(IServiceProvider serviceProvider, ILogger<EventRabbitMqSubscriber> logger, IEventSubscriptionsManager subsManager, IRabbitMqSerializer serializer, IRabbitMqConsumerFactory consumerFactory, IApplicationInstanceInfo application, IOptions<EventBusRabbitMqOptions> mqOptions)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _subsManager = subsManager;
            _serializer = serializer;
            _consumerFactory = consumerFactory;
            _application = application;

            _mqOptions = mqOptions.Value;
            _consumers = new ConcurrentDictionary<string, IRabbitMqConsumer>();
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            foreach (var consumer in _consumers.Values)
            {
                consumer.Unbind(eventName);
            }
        }

        private (string QueueName, int PrefetchCount) ConvertConsumerInfo<TH>()
        {
            var queueName = _mqOptions.QueueName ?? _application.ApplicationName;
            var prefetchCount = _mqOptions.PrefetchCount;
            var consumerAttr = typeof(TH).GetAttribute<EventConsumerAttribute>();
            if (consumerAttr != null)
            {
                queueName = consumerAttr.QueueName;
                prefetchCount = consumerAttr.PrefetchCount <= 0 ? prefetchCount : consumerAttr.PrefetchCount;
            }
            return (queueName, prefetchCount);
        }

        private void DoInternalSubscription(string eventName, string queueName, int prefetchCount)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!_consumers.ContainsKey(queueName))
                {
                    var exchangeDeclareConfiguration = new ExchangeDeclareConfiguration(_mqOptions.ExchangeName, _mqOptions.ExchangeType);
                    var queueDeclareConfiguration = new QueueDeclareConfiguration(queueName, qos: prefetchCount) { Arguments = new Dictionary<string, object> { { "x-queue-mode", "lazy" } } };
                    IRabbitMqConsumer rabbitMqConsumer = _consumerFactory.Create(exchangeDeclareConfiguration, queueDeclareConfiguration);
                    _consumers.TryAdd(queueName, rabbitMqConsumer);
                }

                _consumers.GetOrDefault(queueName)?.Bind(eventName);
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.IsDynamic)
                        {
                            if (!(scope.ServiceProvider.GetRequiredService(subscription.HandlerType) is IDynamicEventHandler handler)) continue;

                            await Task.Yield();
                            await handler.HandleAsync(message);
                        }
                        else
                        {
                            var eventType = _subsManager.GetEventTypeByName(eventName);
                            if (eventType == null) continue;
                            var handler = scope.ServiceProvider.GetRequiredService(subscription.HandlerType);
                            if (handler == null) continue;
                            var integrationEvent = _serializer.Deserialize(message, eventType);
                            var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);

                            await Task.Yield();
                            await (Task)concreteType.GetMethod("HandleAsync").Invoke(handler, new object[] { integrationEvent });
                        }
                    }
                }
            }
        }

        private async Task Consumer_Received(IModel consumer, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            try
            {
                if (message.ToLowerInvariant().Contains("throw-fake-exception"))
                {
                    throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
                }

                await ProcessEvent(eventName, message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", message);
            }
        }

        public void StartConsuming()
        {
            foreach (var queueName in _consumers.Keys)
            {
                _consumers.TryGetValue(queueName, out var _consumer);

                _consumer.OnMessageReceived(Consumer_Received);

                _consumer.StartConsuming();
            }
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IEventHandler<T>
        {
            var eventName = EventNameAttribute.GetNameOrDefault(typeof(T));
            var (QueueName, PrefetchCount) = ConvertConsumerInfo<TH>();

            DoInternalSubscription(eventName, QueueName, PrefetchCount);

            _subsManager.AddSubscription<T, TH>();
        }

        public void SubscribeDynamic<TH>(string eventName) where TH : IDynamicEventHandler
        {
            var (QueueName, PrefetchCount) = ConvertConsumerInfo<TH>();

            DoInternalSubscription(eventName, QueueName, PrefetchCount);

            _subsManager.AddDynamicSubscription<TH>(eventName);
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IEventHandler<T>
        {
            _subsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName) where TH : IDynamicEventHandler
        {
            _subsManager.RemoveDynamicSubscription<TH>(eventName);
        }
    }
}
