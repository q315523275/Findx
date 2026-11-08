using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Reflection;
using Microsoft.Extensions.Logging;

namespace Findx.RabbitMQ;

public class RabbitConsumerBuilder : IRabbitConsumerBuilder
{
    /// <summary>
    /// 消费端
    /// </summary>
    // ReSharper disable once CollectionNeverQueried.Local
    private List<IRabbitMqConsumer> ConsumerList { get; }

    private readonly IRabbitMqConsumerFactory _factory;
    private readonly IRabbitConsumerFinder _finder;
    private readonly IMethodInfoFinder _methodFinder;
    private readonly IRabbitMqSerializer _serializer;
    private readonly ILogger<RabbitConsumerBuilder> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="finder"></param>
    /// <param name="methodFinder"></param>
    /// <param name="factory"></param>
    /// <param name="serializer"></param>
    /// <param name="logger"></param>
    public RabbitConsumerBuilder(IRabbitConsumerFinder finder, IMethodInfoFinder methodFinder, IRabbitMqConsumerFactory factory, IRabbitMqSerializer serializer, ILogger<RabbitConsumerBuilder> logger)
    {
        _finder = finder;
        _methodFinder = methodFinder;
        _factory = factory;
        _serializer = serializer;
        _logger = logger;
        ConsumerList = [];

        // 消息回调在运行期并发执行，缓存必须线程安全
        MethodParameterType = new ConcurrentDictionary<MethodInfo, Type>();
        MethodHandlers = new ConcurrentDictionary<MethodInfo, Func<object, object[], object>>();
    }

    private ConcurrentDictionary<MethodInfo, Type> MethodParameterType { get; }
    private ConcurrentDictionary<MethodInfo, Func<object, object[], object>> MethodHandlers { get; }

    /// <summary>
    ///     构建
    /// </summary>
    public async Task BuildAsync(CancellationToken cancellationToken = default)
    {
        var consumerList = FindAllConsumerExecutor();
        var queueList = consumerList.GroupBy(x => x.QueueName);
        foreach (var group in queueList)
        {
            var defaultQueue = group.First();

            var exchangeDeclareConfiguration = new ExchangeDeclareConfiguration(defaultQueue.ExchangeName, defaultQueue.ExchangeType, defaultQueue.Durable);
            var queueDeclareConfiguration = new QueueDeclareConfiguration(defaultQueue.QueueName, qos: defaultQueue.Qos, durable: defaultQueue.Durable, autoAck: defaultQueue.AutoAck) { Arguments = new Dictionary<string, object> { { "x-queue-mode", "lazy" } } };

            var routingKeyDict = group.GroupBy(x => x.RoutingKey).ToDictionary(x => x.Key, x => x);

            var consumer = _factory.Create(exchangeDeclareConfiguration, queueDeclareConfiguration, defaultQueue.ConnectionName);

            //  接线失败回调：业务处理失败的消息已 Nack(requeue:false) 进入死信，这里记录失败现场
            consumer.FailedCallback = (channel, eventArgs, cancellationToken) =>
            {
                _logger.LogError("RabbitMQ message handling failed, exchange: {Exchange}, routingKey: {RoutingKey}", eventArgs.Exchange, eventArgs.RoutingKey);
                return Task.CompletedTask;
            };

            consumer.OnMessageReceived(async (channel, eventArgs) =>
            {
                // direct模式指定routingKey
                // fanout模式时RabbitConsumer的routingKey需设置为"",发送消息时routingKey需设置为""
                // topic模式暂时模式不支持,可升级为startWith匹配
                var routingKey = eventArgs.RoutingKey;
                if (routingKeyDict.TryGetValue(routingKey, out var typeList))
                {
                    await using var scope = ServiceLocator.CreateAsyncScope();
                    var serviceProvider = scope.ServiceProvider;

                    foreach (var typeInfo in typeList)
                    {
                        var instance = serviceProvider.GetService(typeInfo.Type);
                        var parameterType = MethodParameterType.GetOrAdd(typeInfo.MethodInfo, _ => typeInfo.MethodInfo.GetParameters()[0].ParameterType);
                        var parameter = _serializer.Deserialize(eventArgs.Body, parameterType); 
                        var handler = MethodHandlers.GetOrAdd(typeInfo.MethodInfo, _ => FastInvokeHandler.Create(typeInfo.MethodInfo));

                        var result = handler.Invoke(instance, [parameter, eventArgs, channel]);
                        if (typeInfo.MethodInfo.IsAsync()) await (Task)result;
                    }
                }
            });

            // 循环绑定RoutingKey
            foreach (var item in group.DistinctBy(x => x.RoutingKey))
            {
               await consumer.BindAsync(item.RoutingKey, cancellationToken);
            }

            ConsumerList.Add(consumer);
        }
    }
    
    /// <summary>
    ///     查找所有消费执行者
    /// </summary>
    /// <returns></returns>
    private IEnumerable<ConsumerExecutorDescriptor> FindAllConsumerExecutor()
    {
        var types = _finder.FindAll(true);
        foreach (var type in types)
        foreach (var method in _methodFinder.Find(type, it => it.HasAttribute<RabbitConsumerAttribute>()))
        foreach (var attr in method.GetAttributes<RabbitConsumerAttribute>())
        {
            yield return new ConsumerExecutorDescriptor
            {
                ExchangeType = attr.ExchangeType,
                ExchangeName = attr.ExchangeName,
                ConnectionName = attr.ConnectionName,
                QueueName = attr.QueueName,
                Durable = attr.Durable,
                Qos = attr.Qos,
                RoutingKey = attr.RoutingKey,
                MethodInfo = method,
                Type = type,
                AutoAck = attr.AutoAck
            };
        }
    }
}