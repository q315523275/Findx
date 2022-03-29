using Findx.Extensions;
using Findx.RabbitMQ;
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
    public class EventRabbitMQSubscriber : EventSubscriberBase
    {
        private readonly IRabbitMqConsumerFactory _consumerFactory;
        private readonly IApplicationInstanceInfo _application;
        private readonly EventBusRabbitMqOptions _options;
        private ConcurrentDictionary<string, IRabbitMqConsumer> _consumers;

        public EventRabbitMQSubscriber(IRabbitMqConsumerFactory consumerFactory, IEventSubscribeManager subscribeManager, IEventStore storage, IEventDispatcher dispatcher, IApplicationInstanceInfo application, IOptions<EventBusRabbitMqOptions> options, ILogger<EventSubscriberBase> logger) : base(subscribeManager, storage, logger, dispatcher)
        {
            _consumerFactory = consumerFactory;
            _options = options.Value;
            _application = application;

            _consumers = new ConcurrentDictionary<string, IRabbitMqConsumer>();
        }

        protected override void DoInternalSubscribe(string eventName, string handlerName, int prefetchCount)
        {
            handlerName = handlerName ?? _application.ApplicationName;

            var containsKey = SubscribeManager.HasSubscribeForEvent(eventName);
            if (!containsKey)
            {
                if (!_consumers.ContainsKey(handlerName))
                {
                    var exchangeDeclareConfiguration = new ExchangeDeclareConfiguration(_options.ExchangeName, _options.ExchangeType);

                    var queueDeclareConfiguration = new QueueDeclareConfiguration(handlerName, qos: prefetchCount) { Arguments = new Dictionary<string, object> { { "x-queue-mode", "lazy" } } };

                    IRabbitMqConsumer rabbitMqConsumer = _consumerFactory.Create(exchangeDeclareConfiguration, queueDeclareConfiguration);

                    _consumers.TryAdd(handlerName, rabbitMqConsumer);
                }

                _consumers.GetOrDefault(handlerName)?.BindAsync(eventName);
            }
        }

        protected override void OnEventRemoved(string eventName)
        {
            foreach (var consumer in _consumers.Values)
            {
                consumer.UnbindAsync(eventName);
            }
        }

        protected override void Commit(object eventObject)
        {
            Check.NotNull(eventObject, nameof(eventObject));
            if (eventObject is RabbitMQAcknowledge acknowledge)
            {
                acknowledge.Channel.BasicAck(acknowledge.EventArgs.DeliveryTag, false);
            }
        }

        protected override void Reject(object eventObject)
        {
            Check.NotNull(eventObject, nameof(eventObject));
            if (eventObject is RabbitMQAcknowledge acknowledge)
            {
                acknowledge.Channel.BasicReject(acknowledge.EventArgs.DeliveryTag, true);
            }
        }

        private async Task Consumer_Received(IModel channel, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;

            var headers = new Dictionary<string, string>();
            if (eventArgs.BasicProperties.Headers != null)
            {
                foreach (var header in eventArgs.BasicProperties.Headers)
                {
                    headers.Add(header.Key, header.Value == null ? null : Encoding.UTF8.GetString((byte[])header.Value));
                }
            }

            headers[Headers.EventName] = eventName;

            if (!headers.ContainsKey(Headers.MessageId)) headers[Headers.MessageId] = Guid.NewGuid().ToString();

            var transportMessage = new TransportMessage(headers, eventArgs.Body.ToArray());

            await ProcessAsync(new ProcessingContext(transportMessage, new RabbitMQAcknowledge(channel, eventArgs)));
        }

        public override void StartConsuming()
        {
            foreach (var queueName in _consumers.Keys)
            {
                _consumers.TryGetValue(queueName, out var _consumer);

                _consumer.OnMessageReceived(Consumer_Received);

                // _consumer.StartConsuming();
            }
        }
    }
}
