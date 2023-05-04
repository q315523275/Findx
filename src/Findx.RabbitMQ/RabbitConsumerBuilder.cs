using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Reflection;
using Findx.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.RabbitMQ
{
    public class RabbitConsumerBuilder : IRabbitConsumerBuilder
    {
        private readonly List<IRabbitMqConsumer> _consumerList;
        private readonly IRabbitMqConsumerFactory _factory;
        private readonly IRabbitConsumerFinder _finder;
        private readonly IMethodInfoFinder _methodFinder;
        private readonly IJsonSerializer _serializer;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="finder"></param>
        /// <param name="methodFinder"></param>
        /// <param name="factory"></param>
        /// <param name="serializer"></param>
        public RabbitConsumerBuilder(IRabbitConsumerFinder finder, IMethodInfoFinder methodFinder,
            IRabbitMqConsumerFactory factory, IJsonSerializer serializer)
        {
            _finder = finder;
            _methodFinder = methodFinder;
            _factory = factory;
            _serializer = serializer;
            _consumerList = new List<IRabbitMqConsumer>();

            MethodParameterType = new Dictionary<MethodInfo, Type>();
            MethodHandlers = new Dictionary<MethodInfo, Func<object, object[], object>>();
        }

        private IDictionary<MethodInfo, Type> MethodParameterType { get; }
        private IDictionary<MethodInfo, Func<object, object[], object>> MethodHandlers { get; }

        public void Build()
        {
            var consumerList = FindAllConsumerExecutor();
            var queueList = consumerList.GroupBy(x => x.QueueName);
            foreach (var group in queueList)
            {
                var defaultQueue = group.First();

                var exchangeDeclareConfiguration = new ExchangeDeclareConfiguration(defaultQueue.ExchangeName,
                    defaultQueue.ExchangeType, defaultQueue.Durable);
                var queueDeclareConfiguration =
                    new QueueDeclareConfiguration(defaultQueue.QueueName, qos: defaultQueue.Qos)
                        { Arguments = new Dictionary<string, object> { { "x-queue-mode", "lazy" } } };

                var routingKeyDict = group.GroupBy(x => x.RoutingKey).ToDictionary(x => x.Key, x => x);

                var consumer = _factory.Create(exchangeDeclareConfiguration, queueDeclareConfiguration,
                    defaultQueue.ConnectionName);
                consumer.OnMessageReceived(async (channel, eventArgs) =>
                {
                    // direct模式指定routingKey
                    // fanout模式时RabbitConsumer的routingKey需设置为"",发送消息时routingKey需设置为""
                    // topic模式暂时模式不支持,可升级为startWith匹配
                    var routingKey = eventArgs.RoutingKey;
                    if (routingKeyDict.TryGetValue(routingKey, out var typeList))
                    {
                        var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                        using var scope = ServiceLocator.Instance.CreateScope();
                        var serviceProvider = scope.ServiceProvider;

                        foreach (var typeInfo in typeList)
                        {
                            var instance = serviceProvider.GetService(typeInfo.Type);
                            var parameterType = MethodParameterType.GetOrAdd(typeInfo.MethodInfo,
                                it => typeInfo.MethodInfo.GetParameters()[0].ParameterType);
                            var parameter = ConvertToParameter(parameterType, message);
                            var handler = MethodHandlers.GetOrAdd(typeInfo.MethodInfo,
                                it => FastInvokeHandler.Create(typeInfo.MethodInfo));

                            var result = handler.Invoke(instance, new[] { parameter, eventArgs, channel });
                            if (typeInfo.MethodInfo.IsAsync()) await (Task)result;
                        }
                    }
                });

                // 循环绑定RoutingKey
                foreach (var item in group.DistinctBy2(x => x.RoutingKey)) consumer.BindAsync(item.RoutingKey);

                _consumerList.Add(consumer);
            }
        }

        /// <summary>
        ///     转换参数值
        /// </summary>
        /// <param name="parameterType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private object ConvertToParameter(Type parameterType, string message)
        {
            if (parameterType.IsNullableType()) parameterType = parameterType.GetUnNullableType();

            if (parameterType.IsEnum) return Enum.Parse(parameterType, message);

            if (parameterType == typeof(Guid))
                return TypeDescriptor.GetConverter(parameterType).ConvertFromInvariantString(message);

            if (parameterType == typeof(string)) return message;

            if (parameterType.IsPrimitive && parameterType.IsValueType && parameterType != typeof(char))
                return Convert.ChangeType(message, parameterType);

            return _serializer.Deserialize(message, parameterType);
        }

        /// <summary>
        ///     查找所有消费执行者
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ConsumerExecutorDescriptor> FindAllConsumerExecutor()
        {
            var result = new List<ConsumerExecutorDescriptor>();
            var types = _finder.FindAll(true);
            foreach (var type in types)
            foreach (var method in _methodFinder.Find(type, it => it.HasAttribute<RabbitConsumerAttribute>()))
            foreach (var attr in method.GetAttributes<RabbitConsumerAttribute>())
                result.Add(new ConsumerExecutorDescriptor
                {
                    ExchangeType = attr.ExchangeType,
                    ExchangeName = attr.ExchangeName,
                    ConnectionName = attr.ConnectionName,
                    QueueName = attr.QueueName,
                    Durable = attr.Durable,
                    Qos = attr.Qos,
                    RoutingKey = attr.RoutingKey,
                    MethodInfo = method,
                    Type = type
                });
            return result;
        }
    }
}