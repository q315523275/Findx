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
    public WebSocketMiddleware(RequestDelegate next, WebSocketHandlerBase webSocketHandler, IWebSocketAuthorization webSocketAuthorization)
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
        
        // 无效
        // var cancellationToken = context.RequestAborted;
        var cts = new CancellationTokenSource();
        await WebSocketHandler.OnConnected(webSocketClient, cts.Token).ConfigureAwait(false);
        await ReceiveAsync(webSocketClient, cts.Token);
        #if !NET8_0_OR_GREATER
            cts.Cancel();
        #else
            await cts.CancelAsync(); 
        #endif
    }

    private async Task ReceiveAsync(WebSocketClient client, CancellationToken cancellationToken = default)
    {
        // 缓冲区大小
        var buffer = new byte[2048];
        while (client.Client.State == WebSocketState.Open)
        {
            var arraySegment = new ArraySegment<byte>(buffer);
            try
            {
                string receiveMessage;
                WebSocketReceiveResult result;
                using (var ms = Pool.MemoryStream.Rent())
                {
                    do
                    {
                        result = await client.Client.ReceiveAsync(arraySegment, cancellationToken).ConfigureAwait(false);
                        if (arraySegment.Array != null)
                            ms.Write(arraySegment.Array, arraySegment.Offset, result.Count);
                        
                    } while (!result.EndOfMessage);

                    // 使用固定 byte + MemoryStream方式读取内容,自动组合超过缓冲区内容
                    client.LastHeartbeatTime = DateTime.Now;

                    ms.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        #if !NET8_0_OR_GREATER
                            receiveMessage = await reader.ReadToEndAsync().ConfigureAwait(false);
                        #else
                            receiveMessage = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                        #endif
                    }
                }
                
                // 通过配置可以限定最大并行数量
                HandleAsync(client, result, receiveMessage, cancellationToken).ConfigureAwait(false);
            }
            catch (WebSocketException e)
            {
                if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely) 
                    client.Client.Abort();
            }
        }

        await WebSocketHandler.OnDisconnected(client, cancellationToken);
    }

    private async Task HandleAsync(WebSocketClient client, WebSocketReceiveResult result, string msg, CancellationToken cancellationToken = default)
    {
        if (result.MessageType == WebSocketMessageType.Text)
        {
            await WebSocketHandler.ReceiveAsync(client, result, msg, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (result.MessageType == WebSocketMessageType.Close)
        {
            await WebSocketHandler.OnDisconnected(client, cancellationToken).ConfigureAwait(false);
        }
    }
}