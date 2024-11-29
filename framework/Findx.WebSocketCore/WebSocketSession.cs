using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Findx.Utilities;
using Findx.WebSocketCore.Abstractions;

namespace Findx.WebSocketCore;

/// <summary>
///     WebSocket Session会话
/// </summary>
public class WebSocketSession: IWebSocketSession
{
    private WebSocket _webSocket;

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
    ///     状态
    /// </summary>
    public WebSocketState State => _webSocket.State;

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
    ///     发送消息
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="webSocketMessageType"></param>
    /// <param name="endOfMessage"></param>
    /// <param name="cancellationToken"></param>
    public async Task SendAsync(ReadOnlyMemory<byte> payload, WebSocketMessageType webSocketMessageType = WebSocketMessageType.Text, bool endOfMessage = true, CancellationToken cancellationToken = default)
    {
        await _webSocket.SendAsync(payload, webSocketMessageType, endOfMessage, cancellationToken);
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
    ///     资源释放
    /// </summary>
    public void Dispose()
    {
        _webSocket?.Abort();
       _webSocket?.Dispose();
       _webSocket = null;
    }
}