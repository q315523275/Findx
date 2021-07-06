using Findx.Reflection;
using Findx.Extensions;
using System;
using Findx.DependencyInjection;
using System.Text;
using System.Threading.Tasks;
using Findx.Serialization;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;

namespace Findx.RabbitMQ
{
    public class RabbitConsumerBuilder: IRabbitConsumerBuilder
    {
        private readonly IRabbitConsumerFinder _finder;
        private readonly IMethodInfoFinder _methodFinder;
        private readonly IRabbitMQConsumerFactory _factory;
        private readonly IJsonSerializer _serializer;

        public RabbitConsumerBuilder(IRabbitConsumerFinder finder, IMethodInfoFinder methodFinder, IRabbitMQConsumerFactory factory, IJsonSerializer serializer)
        {
            _finder = finder;
            _methodFinder = methodFinder;
            _factory = factory;
            _serializer = serializer;
            Consumers = new List<IRabbitMQConsumer>();
            MethodParameter = new ConcurrentDictionary<MethodInfo, Type>();
            Handlers = new ConcurrentDictionary<MethodInfo, Func<object, object[], object>>();
        }

        public void Build()
        {
            foreach(var consumer in Consumers)
            {
                consumer.StartConsuming();
            }
        }

        private List<IRabbitMQConsumer> Consumers { get; }
        private ConcurrentDictionary<MethodInfo, Type> MethodParameter { get; }
        private ConcurrentDictionary<MethodInfo, Func<object, object[], object>> Handlers { get; }

        public void Initialize()
        {
            var types = _finder.FindAll(true);

            foreach(var type in types)
            {
                var methods = _methodFinder.Find(type, it => it.HasAttribute<RabbitConsumerAttribute>());
                foreach(var method in methods)
                {
                    var attr = method.GetAttribute<RabbitConsumerAttribute>();
                    var exchangeDeclareConfiguration = new ExchangeDeclareConfiguration(attr.ExchangeName, attr.Type);
                    var queueDeclareConfiguration = new QueueDeclareConfiguration(attr.QueueName, qos: attr.Qos) { Arguments = new Dictionary<string, object> { { "x-queue-mode", "lazy" } } };
                    
                    var consumer = _factory.Create(exchange: exchangeDeclareConfiguration, queue: queueDeclareConfiguration);

                    consumer.Bind(attr.RoutingKey);

                    consumer.OnMessageReceived(async (channel, eventArgs) => {

                        var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                        var instance = ServiceLocator.ServiceProvider.GetService(type);
                        var parameterType = MethodParameter.GetOrAdd(method, it => { return method.GetParameters()[0].ParameterType; });

                        var parameter = _serializer.Deserialize(message, method.GetParameters()[0].ParameterType);

                        Func<object, object[], object> handler = Handlers.GetOrAdd(method, it => { return FastInvokeHandler.Create(method); });

                        var result = handler.Invoke(instance, new object[] { parameter });

                        if (method.IsAsync()) await (Task)result;
                    });

                    Consumers.Add(consumer);
                }
            }
        }
    }
}
