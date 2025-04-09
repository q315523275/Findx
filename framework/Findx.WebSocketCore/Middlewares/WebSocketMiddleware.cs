using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Findx.Security;
using Findx.WebSocketCore.Abstractions;
using Findx.WebSocketCore.Extensions;
using Microsoft.AspNetCore.Http;

namespace Findx.WebSocketCore.Middlewares;

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
        if (!await _webSocketAuthorization.AuthorizeAsync(context))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("basic auth failed .");
            return;
        }
        
        // 控制连接
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var webSocketSession = new WebSocketSession(webSocket)
        {
            RemoteIpAddress = context.GetRemoteIpAddress(),
            RemotePort = context.Connection.RemotePort,
            UserName = context.Request.Query.TryGetValue("userName", out var value) ? value.ToString() : "匿名"
        };
        
        // 认证通过进行用户名赋值
        if (context.User.Identity?.IsAuthenticated == true)
        {
            webSocketSession.UserName = context.User.Identity?.GetUserName();
        }
        
        // 无效
        // var cancellationToken = context.RequestAborted;
        var cts = new CancellationTokenSource();
        await WebSocketHandler.OnConnected(webSocketSession, cts.Token).ConfigureAwait(false);
        await ReceiveAsync(webSocketSession, cts.Token);
        #if NET8_0_OR_GREATER
            await cts.CancelAsync(); 
        #else
            cts.Cancel();
        #endif
    }
    
    private async Task ReceiveAsync(IWebSocketSession session, CancellationToken cancellationToken = default)
    {
        while (session.State == WebSocketState.Open)
        {
            // 4kb缓冲区大小
            const int chunkSize = 1024 * 4;
            var bytes = ArrayPool<byte>.Shared.Rent(chunkSize);
            var buffer = new Memory<byte>(bytes);

            try
            {
                var result = await session.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await HandleAsync(session, null, result, cancellationToken);
                    return;
                }
                else
                {
                    if (result.EndOfMessage)
                    {
                        if (result.Count > 0)
                        {
                            var message = ResponseMessage.BinaryMessage(buffer[..result.Count]);
                            await HandleAsync(session, message, result, cancellationToken);
                        }
                    }
                    else
                    {
                        await using var ms = Pool.MemoryStream.Rent();
                        await ms.WriteAsync(buffer[..result.Count], cancellationToken);
                        do
                        {
                            result = await session.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                            if (result.Count > 0) await ms.WriteAsync(buffer[..result.Count], cancellationToken);
                        } while (!result.EndOfMessage);
                        ms.Seek(0, SeekOrigin.Begin);
                        var message = ResponseMessage.BinaryStreamMessage(ms);
                        await HandleAsync(session, message, result, cancellationToken);
                    }
                }
            }
            catch (WebSocketException e)
            {
                if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    session.CloseAsync(WebSocketCloseStatus.InternalServerError, "Connection closed prematurely", CancellationToken.None);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }
    
        await WebSocketHandler.OnDisconnected(session, cancellationToken);
    }

    private async Task HandleAsync(IWebSocketSession session, ResponseMessage message, ValueWebSocketReceiveResult receiveResult, CancellationToken cancellationToken = default)
    {
        switch (receiveResult.MessageType)
        {
            case WebSocketMessageType.Text or WebSocketMessageType.Binary:
                await WebSocketHandler.ReceiveAsync(session, message, receiveResult, cancellationToken).ConfigureAwait(false);
                return;
            
            case WebSocketMessageType.Close:
                await WebSocketHandler.OnDisconnected(session, cancellationToken).ConfigureAwait(false);
                break;
        }
    }
}