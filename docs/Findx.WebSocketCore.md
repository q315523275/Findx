# Findx.WebSocketCore

WebSocket 组件，基于 `System.Net.WebSockets` 实现自定义 WebSocket 功能，其中包括 WebSocket 客户端与服务端、客户端连接管理等功能。

> Nuget 包引用

```
Findx.WebSocketCore
```

目标框架：`net6.0` / `net8.0` / `net10.0`。

本包提供服务端（`WebSocketHandlerBase` + 中间件 + 会话管理）与客户端（`Hubs/Client`）两侧能力。

> 装配方式

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 WebSocketCoreModule
```

`WebSocketCoreModule`（`ModuleLevel.Application`，`Order = 60`）注册：

```csharp
services.AddSingleton<IWebSocketSessionManager, WebSocketSessionManager>();
services.AddSingleton<IWebSocketSerializer, WebSocketSerializer>();
services.AddSingleton<IWebSocketAuthorization, DefaultWebSocketAuthorization>();

// 自动发现所有 WebSocketHandlerBase 具体子类并注册为单例
var finder = services.GetOrAddSingletonInstance<IWebSocketHandlerTypeFinder>(() => new WebSocketHandlerTypeFinder());
foreach (var type in finder.FindAll())
    services.AddSingleton(type);
```

发现器会排除被继承的中间基类，只注册最具体的 Handler。

> 本包没有独立的配置节

心跳 `KeepAliveInterval` 来自 ASP.NET Core 自带的 `WebSocketOptions`，不是本包的选项：

```csharp
builder.Services.AddWebSockets(x => { x.KeepAliveInterval = TimeSpan.FromMinutes(2); });
```

## 服务端

> 定义 Handler

```csharp
using System.Net.WebSockets;
using Findx.WebSocketCore;
using Findx.WebSocketCore.Abstractions;
using Findx.WebSocketCore.Extensions;

public class WebSocketHandler : WebSocketHandlerBase
{
    public WebSocketHandler(IWebSocketSessionManager sessionManager)
        : base(sessionManager) { }

    // 唯一必须重写的方法
    public override async Task ReceiveAsync(IWebSocketSession client, ResponseMessage message,
        ValueWebSocketReceiveResult result, CancellationToken cancellationToken = default)
    {
        var text = await message.AsTextAsync(cancellationToken);

        await SendMessageAsync(client, new RequestTextMessage(text), cancellationToken);
    }
}
```

> 挂载端点

```csharp
app.UseWebSockets().MapWebSocket("/ws", app.Services.GetRequiredService<WebSocketHandler>());
```

客户端连接 `ws://127.0.0.1:5001/ws?userName=张三`。

> 注意事项

方法名是 **`MapWebSocket`**，不是 `MapWebSocketManager`。`MapWebSocket` 内部通过 `app.Map(...)` 建立分支管道。

## 连接与断开

`WebSocketHandlerBase` 的两个虚方法都有默认实现，**覆写时必须调用 base**，否则连接不会被登记，后续广播会找不到该连接。

```csharp
public override async Task OnConnected(IWebSocketSession session, CancellationToken ct = default)
{
    await base.OnConnected(session, ct);              // base 内部会 AddSession 并回一条欢迎文本
    SessionManager.AddToGroup(session, "lobby");
    Console.WriteLine($"在线用户数: {SessionManager.Count}");
}

public override async Task OnDisconnected(IWebSocketSession session, CancellationToken ct = default)
{
    SessionManager.RemoveFromGroup(session, "lobby");
    await base.OnDisconnected(session, ct);
}
```

> 用户名优先级

```
1. 已认证 → identity.GetUserName()
2. 查询字符串 → Request.Query["userName"]
3. 都无 → "匿名"
```

## 发送消息

五个发送重载均已实现，无需覆写。

```csharp
await SendMessageAsync(client, message, ct);                              // 单个连接
await SendMessageAsync("bob", message, ct);                               // 某用户全部连接
await SendMessageToAllAsync(message, ct);                                 // 全部连接
await SendMessageToGroupAsync("room-1", message, ct);                      // 组内全部
await SendMessageToGroupAsync("room-1", message, "bob", ct);               // 组内排除指定用户
```

> 消息类型

```csharp
new RequestTextMessage(text);                              // 文本
new RequestBinaryMessage(bytes);                            // 二进制
new RequestBinarySegmentMessage(new ArraySegment<byte>(b)); // 分片二进制
```

> 注意事项

发送时无论消息类型，`WebSocketMessageType` 一律传 `Text`；目标连接状态非 `Open` 时直接跳过。

## 接收消息

服务端接收到的始终是 `ResponseMessage`，通过扩展方法转换为具体类型。

```csharp
public override async Task ReceiveAsync(IWebSocketSession client, ResponseMessage message,
    ValueWebSocketReceiveResult result, CancellationToken cancellationToken = default)
{
    // 文本
    var text = await message.AsTextAsync(cancellationToken);

    // JSON 反序列化（自动判断 Memory / Stream）
    var dto = message.Deserialize<ChatDto>();

    // 直接访问底层数据
    ReadOnlyMemory<byte> memory = message.Memory;
    MemoryStream? stream = message.Stream;
}
```

> 分片处理

中间件按 4KB 分片接收，使用 `ArrayPool<byte>` 与池化 `MemoryStream` 拼接分片消息，完整后才回调 Handler。

## 会话管理

```csharp
public interface IWebSocketSessionManager
{
    int Count { get; }                                             // 用户数
    IWebSocketSession GetSession(string userName);                 // 该用户全部会话
    IEnumerable<IWebSocketSession> GetAllSessions();
    IEnumerable<string> GetAllUserFromGroup(string groupId);       // 组内用户名
    void AddSession(IWebSocketSession session);
    void AddToGroup(IWebSocketSession session, string groupId);
    void RemoveFromGroup(IWebSocketSession session, string groupId);
    Task RemoveSessionAsync(string userName, CancellationToken ct = default);
    Task RemoveSessionAsync(IWebSocketSession session, CancellationToken ct = default);
}
```

> 使用

```csharp
// 踢掉某用户全部连接
await SessionManager.RemoveSessionAsync("bob");

// 踢掉单个连接
await SessionManager.RemoveSessionAsync(session);

// 在线统计
Console.WriteLine(SessionManager.Count);
```

> 注意事项

- `Count` 返回的是**用户数**而非连接数，同一用户多端连接只算 1。
- 组内只存 `userName`，同一用户多端时无法在组内精确区分某条连接。
- `RemoveSessionAsync(session)` **不会**同步从分组中清理（只有应用停机时的全量清理会清空）。
- 会话数据全部在进程内存中，应用停止时统一优雅关闭。

## 分组（频道）

分组即频道，没有独立的 Channel 抽象。

```csharp
SessionManager.AddToGroup(client, "room-1");
SessionManager.RemoveFromGroup(client, "room-1");

var users = SessionManager.GetAllUserFromGroup("room-1");   // 组不存在返回 null
await SendMessageToGroupAsync("room-1", message, ct);
```

## 鉴权

`IWebSocketAuthorization` 复用 [Findx.Security](Findx.Security.md) 的 `JwtOptions` 校验请求头。

```csharp
public interface IWebSocketAuthorization
{
    Task<bool> AuthorizeAsync(HttpContext context);
}
```

> 默认实现行为

| 场景 | 结果 |
|---|---|
| 请求头含 `Authorization` | 校验 JWT，成功则挂到 `context.User` |
| 无 `Authorization` 头 | **放行**（返回 `true`） |

> 注意事项

- 需先开启 `Findx:Authentication:Jwt:Enabled` 并配好 `Secret`，否则构造函数会抛异常。
- 浏览器 `WebSocket` API **无法自定义请求头**，实际需用查询字符串传 Token，可继承 `DefaultWebSocketAuthorization` 重写为从 `Request.Query` 读取。

> 自定义鉴权

```csharp
public class QueryTokenAuthorization : DefaultWebSocketAuthorization
{
    public QueryTokenAuthorization(IOptions<JwtOptions> options) : base(options) { }

    public override Task<bool> AuthorizeAsync(HttpContext context)
    {
        var token = context.Request.Query["token"].ToString();
        if (token.IsNullOrWhiteSpace()) return Task.FromResult(false);
        // 校验 token...
        return Task.FromResult(true);
    }
}
```

```csharp
services.Replace(ServiceDescriptor.Singleton<IWebSocketAuthorization, QueryTokenAuthorization>());
```

## 客户端

> 基本用法

```csharp
using Findx.WebSocketCore.Hubs.Client;
using Findx.WebSocketCore.Extensions;

var connection = new HubConnectionBuilder()
    .WithUrl("ws://127.0.0.1:5001/ws?userName=开发")
    .WithAutomaticReconnection()
    .Build();

await connection.StartAsync();

// 接收
connection.On(async (message, token) =>
{
    var text = await message.AsTextAsync(token);
    Console.WriteLine($"收到: {text}");
});

// 事件
connection.Closed       += error => { Console.WriteLine(error?.Message); return Task.CompletedTask; };
connection.Reconnecting += () => Task.CompletedTask;
connection.Reconnected  += () => Task.CompletedTask;

// 发送
await connection.SendAsync(new RequestTextMessage("hello"), WebSocketMessageType.Text, true);
await connection.SendAsync(new RequestBinaryMessage(bytes), WebSocketMessageType.Binary, true);

// 关闭
await connection.StopAsync();
```

> 自定义请求头

```csharp
connection.SetRequestHeader("Authorization", "Bearer " + token);
```

> 客户端基类

适合做 Windows Service、后台任务或 CLI 客户端。

```csharp
public class MyHubClient : HubClientBase
{
    public override string HubUrl => "ws://127.0.0.1:5001/ws";

    public override void Initialize()
    {
        base.Initialize();                                       // 建连接 + 挂事件
        SetRequestHeader("Authorization", "Bearer " + token);    // 需在 base 之后
        HubListenOn(HubConnection);                             // 内部绑定消息回调
    }
}
```

`HubClientBase` 提供 `StartAsync` / `StopAsync` / `ReStartAsync`，以及 `IsHubConnected`、`IsHubConnecting` 状态判断。连接失败时按随机间隔自动重试。

> 浏览器端

```javascript
const ws = new WebSocket(`ws://127.0.0.1:5001/ws?userName=${encodeURIComponent(name)}`);
ws.onopen    = () => ws.send("hello");
ws.onmessage = e => console.log("recv:", e.data);
ws.onclose   = () => console.log("closed");

// 二进制
ws.binaryType = "arraybuffer";
ws.send(new Uint8Array([1, 2, 3]));
```

## 分布式扩展

框架**未内置** Redis 发布订阅等分布式广播实现，当前会话管理为单进程内存态。csproj 描述中的「可扩展为分布式」指的是留有清晰的扩展点：

| 扩展点 | 做法 |
|---|---|
| 会话管理 | 实现自定义 `IWebSocketSessionManager`，把字典换成 Redis Hash / Set，覆盖注册 |
| 跨节点广播 | 将 `SendMessageToAllAsync` / `SendMessageToGroupAsync` 改为「先通过 Redis 或 RabbitMQ 广播指令，各节点订阅后投递本地会话」 |
| 负载均衡 | WebSocket 握手需落在固定节点，可复用 [Findx.Discovery](Findx.Discovery.md) 的一致性哈希算法 |
| 消息总线 | 用 [Findx.RabbitMQ](Findx.RabbitMQ.md) 替代 Redis 发布订阅 |

依赖 [Findx.Security](Findx.Security.md) 的原因仅为复用 `JwtOptions` 做鉴权。