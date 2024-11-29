using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebSocketCore.Abstractions;

/// <summary>
///     会话
/// </summary>
public interface IWebSocketSession: IDisposable
{
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
    ///     
    /// </summary>
    public string UserName { get; set; }
    
    /// <summary>
    ///     状态
    /// </summary>
    public WebSocketState State { get; }

    /// <summary>
    ///     接收消息
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken);
    
    /// <summary>
    ///     发送消息
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="webSocketMessageType"></param>
    /// <param name="endOfMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAsync(ReadOnlyMemory<byte> payload, WebSocketMessageType webSocketMessageType = WebSocketMessageType.Text, bool endOfMessage = true, CancellationToken cancellationToken = default);

    /// <summary>
    ///     关闭连接
    /// </summary>
    /// <param name="closeStatus"></param>
    /// <param name="statusDescription"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken = default);
}
