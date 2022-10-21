using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Reflection;
using Findx.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Findx.RabbitMQ
{
    public class RabbitConsumerBuilder : IRabbitConsumerBuilder
    {
        private readonly IRabbitConsumerFinder _finder;
        private readonly IMethodInfoFinder _methodFinder;
        private readonly IRabbitMqConsumerFactory _factory;
        private readonly IJsonSerializer _serializer;
        private readonly List<IRabbitMqConsumer> _consumers;

        public RabbitConsumerBuilder(IRabbitConsumerFinder finder, IMethodInfoFinder methodFinder, IRabbitMqConsumerFactory factory, IJsonSerializer serializer)
        {
            _finder = finder;
            _methodFinder = methodFinder;
            _factory = factory;
            _serializer = serializer;
            _consumers = new List<IRabbitMqConsumer>();
            MethodParameter = new Dictionary<MethodInfo, Type>();
            Handlers = new Dictionary<MethodInfo, Func<object, object[], object>>();
        }

        public void Build()
        {
            var types = _finder.FindAll(true);

            foreach (var type in types)
            {
                var methods = _methodFinder.Find(type, it => it.HasAttribute<RabbitConsumerAttribute>());
                foreach (var method in methods)
                {
                    var attr = method.GetAttribute<RabbitConsumerAttribute>();
                    var exchangeDeclareConfiguration = new ExchangeDeclareConfiguration(attr.ExchangeName, attr.Type, durable: attr.Durable);
                    var queueDeclareConfiguration = new QueueDeclareConfiguration(attr.QueueName, qos: attr.Qos) { Arguments = new Dictionary<string, object> { { "x-queue-mode", "lazy" } } };

                    var consumer = _factory.Create(exchange: exchangeDeclareConfiguration, queue: queueDeclareConfiguration, attr.ConnectionName);

                    consumer.OnMessageReceived(async (channel, eventArgs) =>
                    {
                        var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                        using var scope = ServiceLocator.Instance.CreateScope();
                        var serviceProvider = scope.ServiceProvider;

                        var instance = serviceProvider.GetService(type);

                        var parameterType = MethodParameter.GetOrAdd(method, it => method.GetParameters()[0].ParameterType);

                        var parameter = ConvertToParameter(parameterType, message);

                        var handler = Handlers.GetOrAdd(method, it => FastInvokeHandler.Create(method));

                        var result = handler.Invoke(instance, new[] { parameter, eventArgs, channel });

                        if (method.IsAsync()) await (Task)result;
                    });

                    consumer.BindAsync(attr.RoutingKey);

                    _consumers.Add(consumer);
                }
            }
        }
        private IDictionary<MethodInfo, Type> MethodParameter { get; }
        private IDictionary<MethodInfo, Func<object, object[], object>> Handlers { get; }

        /// <summary>
        /// 转换参数值
        /// </summary>
        /// <param name="parameterType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private object ConvertToParameter(Type parameterType, string message)
        {
            if (parameterType.IsNullableType())
            {
                parameterType = parameterType.GetUnNullableType();
            }

            if (parameterType.IsEnum)
            {
                return Enum.Parse(parameterType, message);
            }

            if (parameterType == typeof(Guid))
            {
                return TypeDescriptor.GetConverter(parameterType).ConvertFromInvariantString(message);
            }

            if(parameterType == typeof(string))
            {
                return message;
            }
            
            if(parameterType.IsPrimitive && parameterType.IsValueType && parameterType != typeof(char))
            {
                return Convert.ChangeType(message, parameterType);
            }

            return _serializer.Deserialize(message, parameterType);
        }
    }
}
