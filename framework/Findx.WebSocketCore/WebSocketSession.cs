using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Utilities;
using Findx.WebSocketCore.Abstractions;
using Microsoft.Extensions.Logging;

namespace Findx.WebSocketCore;

/// <summary>
///     WebSocket Session会话
/// </summary>
public class WebSocketSession: IWebSocketSession
{
    private WebSocket _webSocket;

    /// <summary>
    ///     发送串行化锁：.NET WebSocket 禁止对同一实例并发 SendAsync
    /// </summary>
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    /// <summary>
    ///     Dispose幂等标记（0=未释放，1=已释放）
    /// </summary>
    private int _disposed;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="webSocket"></param>
    public WebSocketSession(WebSocket webSocket)
    {
        _webSocket = webSocket;

        Id = SnowflakeIdUtility.Default().NextId();
    }

    /// <summary>
    ///     会话编号
    /// </summary>
    public long Id { get; }

    /// <summary>
    ///     远程IP地址
    /// </summary>
    public string RemoteIpAddress { get; set; }

    /// <summary>
    ///     远程端口
    /// </summary>
    public int RemotePort { get; set; }

    /// <summary>
    ///     用户名称
    /// </summary>
    public string UserName { get; set; } = "temporary";

    /// <summary>
    ///     状态（释放后视为Closed，避免调用方NRE）
    /// </summary>
    public WebSocketState State => _webSocket?.State ?? WebSocketState.Closed;

    /// <summary>
    ///     接收数据
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        return await _webSocket.ReceiveAsync(buffer, cancellationToken);
    }

    /// <summary>
    ///     发送消息（信号量串行化，杜绝同socket并发发送）
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="webSocketMessageType"></param>
    /// <param name="endOfMessage"></param>
    /// <param name="cancellationToken"></param>
    public async Task SendAsync(ReadOnlyMemory<byte> payload, WebSocketMessageType webSocketMessageType = WebSocketMessageType.Text, bool endOfMessage = true, CancellationToken cancellationToken = default)
    {
        await _sendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            //  已释放后禁止再发送
            if (_webSocket == null) return;

            await _webSocket.SendAsync(payload, webSocketMessageType, endOfMessage, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    /// <summary>
    ///     关闭连接
    /// </summary>
    /// <param name="closeStatus"></param>
    /// <param name="statusDescription"></param>
    /// <param name="cancellationToken"></param>
    public async Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken = default)
    {
        await _webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        //  幂等：仅允许一个执行路径完成释放
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

        try
        {
            if (_webSocket != null)
            {
                //  主动断开关闭，CloseSent再次通知
                if (_webSocket.State is WebSocketState.Open or WebSocketState.CloseSent)
                {
                    try
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by server", CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogCloseException(ex);
                        _webSocket.Abort();
                    }
                }
                //  被动关闭ack
                else if (_webSocket.State is WebSocketState.CloseReceived)
                {
                    try
                    {
                        await _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledged by server", CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogCloseException(ex);
                        _webSocket.Abort();
                    }
                }

                _webSocket.Dispose();
            }
        }
        finally
        {
            _webSocket = null;
            _sendLock.Dispose();
        }
    }

    /// <summary>
    ///     关闭异常日志（与HubConnection保持同一日志口径）
    /// </summary>
    private static void LogCloseException(Exception ex)
    {
        if (ServiceLocator.IsProviderEnabled)
            ServiceLocator.GetService<ILogger<WebSocketSession>>()?.LogWarning(ex, "WebSocketSession关闭连接异常，已转Abort");
        else
            Console.WriteLine($"WebSocketSession关闭连接异常，已转Abort:{ex.Message}");
    }
}
