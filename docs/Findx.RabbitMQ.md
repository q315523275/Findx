# Findx.RabbitMQ

RabbitMQ 队列组件，基于 RabbitMQ.Client 7.x 异步客户端，实现属性标记（队列特性）的队列消息消费方式，实现异常处理回调，自定义消息序列化器。

> Nuget 包引用

```
Findx.RabbitMQ
```

目标框架：`net6.0` / `net8.0` / `net10.0`，底层依赖 `RabbitMQ.Client 7.2.2`。

> 本包无独立的 DI 扩展方法

通过 `StartupModule` 机制自动装配：

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 FindxRabbitMqModule
```

`FindxRabbitMqModule`（`ModuleLevel.Framework`，`Order = 100`）注册：

```csharp
services.AddSingleton<IConnectionPool, ConnectionPool>();
services.AddSingleton<IChannelPool, ChannelPool>();
services.AddSingleton<IRabbitMqConsumerFactory, RabbitMqConsumerFactory>();
services.AddTransient<RabbitMqConsumer>();
services.AddSingleton<IRabbitMqSerializer, DefaultRabbitMqSerializer>();
services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
services.AddSingleton<IRabbitConsumerBuilder, RabbitConsumerBuilder>();
services.AddSingleton<IRabbitConsumerFinder, RabbitConsumerFinder>();
services.AddHostedService<RabbitConsumerBuildWorker>();
```

> 重要

开关读取的是 `IConfiguration` 原始值（`GetValue<bool>`），**必须显式配置 `"Enabled": true`**。

## Json 配置

```json
{
    "Findx": {
        "RabbitMQ": {
            "Enabled": true,
            "Connections": {
                "Default": {
                    "HostName": "localhost",
                    "Port": 5672,
                    "UserName": "guest",
                    "Password": "guest",
                    "VirtualHost": "/"
                },
                "Order": {
                    "HostName": "rmq-order.internal",
                    "Port": 5672,
                    "UserName": "u",
                    "Password": "p",
                    "VirtualHost": "/order"
                }
            }
        }
    }
}
```

`Connections` 每个子键对应 `RabbitMQ.Client.ConnectionFactory` 的一个可绑定属性（`HostName`、`Port`、`UserName`、`Password`、`VirtualHost`、`RequestedHeartbeat`、`AutomaticRecoveryEnabled` 等）。

连接实例由 `ConnectionPool` 按连接名缓存复用，`HostName` 支持用 `;` 分隔多主机。

## 定义消费者

两个特性缺一不可：类上 `[RabbitListener]`，方法上 `[RabbitConsumer]`。

> 使用示例

```csharp
[Dependency(ServiceLifetime.Transient, TryRegister = true, AddSelf = true, ReplaceServices = true)]
[RabbitListener]
public class DefaultConsumer
{
    [RabbitConsumer("findx_event_bus", "direct", "Findx.Consumer", 1, "Findx.WebHost.EventBus.FindxTestEvent")]
    public async Task<string> ReceiveMessage(FindxTestEvent message)
    {
        Console.WriteLine($"收到消息：{message.ToJson()}");
        await Task.Delay(3);
        return "1";
    }

    [RabbitConsumer("findx_event_bus", "direct", "Findx.Consumer", 1, "Findx.WebHost.RabbitConsumer")]
    public async Task<string> ReceiveMessage(string message)
    {
        await Task.Delay(3);
        Console.WriteLine($"收到消息：{message}");
        return "222222";
    }
}
```

> 特性说明

`RabbitListenerAttribute`（类级）是纯标记，无属性，用于被扫描器发现。

`RabbitConsumerAttribute`（方法级）：

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RabbitConsumerAttribute : Attribute
{
    public RabbitConsumerAttribute(string exchangeName, string exchangeType, string queueName,
                                   int qos, string routingKey, string connectionName = null);

    public string ExchangeName { get; }               // 必填
    public string ExchangeType { get; } = "direct";   // direct / fanout / topic
    public bool   Durable { set; get; } = false;      // 交换机是否持久化
    public string QueueName { get; }                 // 必填
    public int    Qos { get; set; } = 1;             // 最大同时接收数量
    public string RoutingKey { set; get; }           // 必填
    public string ConnectionName { set; get; }        // 指定 Connections 里的连接名
    public bool   AutoAck { set; get; } = true;      // 自动 Ack 回执
}
```

> 注意事项

- `[Dependency]` 特性不能省略——`RabbitConsumerBuilder` 通过 DI 解析消费者实例，未注册会解析失败。
- 消费方法签名为 `(TMessage message, BasicDeliverEventArgs eventArgs, IChannel channel)`，返回值可忽略。
- 消息类型由**方法第一个参数的类型**决定，配合自定义序列化器完成反序列化。

## 发布消息

```csharp
[HttpGet("publish")]
public async Task<CommonResult> PublishAsync([FromServices] IRabbitMqPublisher publisher,
    [Required] string message, [Required] string exchangeName,
    [Required] string exchangeType, [Required] string routingKey, CancellationToken ct)
{
    await publisher.PublishAsync(new FindxTestEvent { Message = message },
                                 exchangeName, exchangeType, routingKey, cancellationToken: ct);
    return CommonResult.Success();
}
```

> 接口定义

```csharp
public interface IRabbitMqPublisher
{
    Task PublishAsync<T>(T obj, string exchangeName, string exchangeType, string routingKey,
                         bool durable = false, bool autoDelete = false,
                         CancellationToken cancellationToken = default);
}
```

> 实现要点

| 项 | 说明 |
|---|---|
| 通道 | 每次发布单独开通道，用完即弃 |
| 连接 | 走**默认连接**，不支持指定连接名 |
| 投递模式 | `DeliveryModes.Persistent` 持久化投递 |
| 关联 Id | `CorrelationId` 为空时取 `ICorrelationIdProvider.Get()` |
| 交换机声明 | 每次发布前 `ExchangeDeclareAsync` |

## 消息序列化

```csharp
public interface IRabbitMqSerializer
{
    byte[] Serialize<T>(T obj);
    object Deserialize(ReadOnlyMemory<byte> value, Type type);
}
```

默认实现 `DefaultRabbitMqSerializer` 为**零反射快速路径**：

| 类型 | 处理方式 |
|---|---|
| `byte[]` | 直接返回 |
| `string` | UTF-8 字节（≤256 字节用 `stackalloc`） |
| 基本类型 / `decimal` | `ToString()` 转字节；读取时 `Utf8Parser.TryParse` 优先 |
| 其他 | System.Text.Json 序列化 / 反序列化 |

> 自定义序列化器

```csharp
public class ProtobufMqSerializer : IRabbitMqSerializer
{
    public byte[] Serialize<T>(T obj) => ProtobufSerializer.SerializeToBytes(obj);
    public object Deserialize(ReadOnlyMemory<byte> value, Type type)
        => ProtobufSerializer.Deserialize(type, value.Span);
}
```

```csharp
services.AddSingleton<IRabbitMqSerializer, ProtobufMqSerializer>();
```

## 消费者管理

> 自动构建

`RabbitConsumerBuildWorker`（`BackgroundService`）在应用启动时扫描并构建全部消费者：

1. 扫描 `[RabbitListener]` 类 → 找带 `[RabbitConsumer]` 的方法
2. **按 `QueueName` 分组**（同一队列共用一个消费者与通道）
3. 声明交换机与队列（队列参数 `x-queue-mode = lazy`）
4. 创建消费者并按 `RoutingKey` 逐个绑定

> 消费流程

```csharp
consumer.OnMessageReceived((channel, eventArgs) =>
{
    // 1. 按 eventArgs.RoutingKey 精确匹配到具体方法
    // 2. ServiceLocator.CreateAsyncScope() 创建独立作用域
    // 3. 反序列化消息体为方法第一个参数的类型
    // 4. FastInvokeHandler 反射调用；若方法是异步则 await
});
```

> 自动重连

`RabbitMqConsumer` 内部启动一个 5 秒周期的定时器，检测到通道关闭时自动重建并重新绑定。

> Ack 策略

- 使用 `BasicConsume(autoAck: false)` 先手动应答
- 业务处理成功或**抛出异常**时都会 `BasicAck`，避免重复投递
- 需要重试场景需自行在回调中 `BasicNack`

> 异常回调

```csharp
IRabbitMqConsumer consumer = ...;
consumer.FailedCallback = async (channel, eventArgs, ct) =>
{
    // 记录失败日志 / 告警
};
```

异常同时会走 `IExceptionNotifier` 广播给所有 `IExceptionSubscriber`。

> 编程式消费者

```csharp
var consumer = consumerFactory.Create(
    new ExchangeDeclareConfiguration("my_ex", "direct", durable: true),
    new QueueDeclareConfiguration("my_queue", qos: 5),
    connectionName: "Order");

consumer.FailedCallback = async (ch, ea, ct) => { /* 记录失败 */ };
consumer.OnMessageReceived(async (ch, ea) => { /* 处理消息 */ });

await consumer.BindAsync("order.created", ct);
await consumer.UnbindAsync("order.created", ct);
```

## 连接与通道池

| 类型 | 说明 |
|---|---|
| `ConnectionPool` | 按连接名缓存 `IConnection`，命中且 `IsOpen` 直接复用，已关闭则重建 |
| `ChannelPool` | 虚拟通道独占池，`AcquireAsync` 阻塞等待可用通道，`Release` 归还 |

`ChannelPool` 在销毁时有 10 秒等待预算，超时未归还的通道会打 Warning 日志后强制释放。

## 注意事项

- **路由匹配为精确匹配**（按 `eventArgs.RoutingKey`），源码明确 **topic 通配符模式暂不支持**。
- `fanout` 交换机下 `RoutingKey` 需为空字符串。
- `RabbitMqPublisher.PublishAsync` 走默认连接，无 `connectionName` 参数。
- 消费者按队列分组，同一队列的多个消费方法共用一个通道。