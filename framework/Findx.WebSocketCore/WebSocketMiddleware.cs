using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Findx.Utilities;
using Microsoft.AspNetCore.Http;

namespace Findx.WebSocketCore;

/// <summary>
///     中间件
/// </summary>
public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;

    private readonly IWebSocketAuthorization _webSocketAuthorization;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="next"></param>
    /// <param name="webSocketHandler"></param>
    /// <param name="webSocketAuthorization"></param>
    public WebSocketMiddleware(RequestDelegate next, WebSocketHandlerBase webSocketHandler,
        IWebSocketAuthorization webSocketAuthorization)
    {
        WebSocketHandler = webSocketHandler;
        _next = next;
        _webSocketAuthorization = webSocketAuthorization;
    }

    private WebSocketHandlerBase WebSocketHandler { get; }

    /// <summary>
    ///     执行
    /// </summary>
    /// <param name="context"></param>
    public async Task Invoke(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            await _next.Invoke(context);
            return;
        }

        // 开放认证
        if (!await _webSocketAuthorization.AuthorizeAsync(context.Request))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("basic auth failed .");
            return;
        }

        // 控制连接
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var webSocketClient = new WebSocketClient
        {
            Client = webSocket,
            Id = context.Request.Query.TryGetValue("id", out var id) ? id : Guid.NewGuid().ToString(),
            Name = context.Request.Query.TryGetValue("name", out var name) ? name : "未知",
            Tag = context.Request.Query.TryGetValue("tag", out var tag) ? tag : "",
            Environment = context.Request.Query.TryGetValue("environment", out var environment) ? environment : "",
            RemoteIp = context.GetClientIp(),
            ServerIp = HostUtility.ResolveHostAddress(HostUtility.ResolveHostName()),
            LastHeartbeatTime = DateTime.Now
        };
        await WebSocketHandler.OnConnected(webSocketClient).ConfigureAwait(false);

        await ReceiveAsync(webSocketClient, async (result, serializedMessage) =>
        {
            if (result.MessageType == WebSocketMessageType.Text)
            {
                await WebSocketHandler.ReceiveAsync(webSocketClient, result, serializedMessage).ConfigureAwait(false);
                return;
            }

            if (result.MessageType == WebSocketMessageType.Close)
                await WebSocketHandler.OnDisconnected(webSocketClient);
        });
    }

    private async Task ReceiveAsync(WebSocketClient client, Func<WebSocketReceiveResult, string, Task> handleMessage)
    {
        while (client.Client.State == WebSocketState.Open)
        {
            var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
            try
            {
                string message;
                WebSocketReceiveResult result;
                using (var ms = Pool.MemoryStream.Rent())
                {
                    do
                    {
                        result = await client.Client.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                        if (buffer.Array != null)
                            ms.Write(buffer.Array, buffer.Offset, result.Count);
                    } while (!result.EndOfMessage);
                    
                    // 可以直接使用buffer读取内容
                    // 这里使用固定byte+MemoryStream方式读取内容

                    client.LastHeartbeatTime = DateTime.Now;

                    ms.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        message = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }

                handleMessage(result, message);
            }
            catch (WebSocketException e)
            {
                if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely) client.Client.Abort();
            }
        }

        await WebSocketHandler.OnDisconnected(client);
    }
}