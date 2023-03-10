using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebSocketCore;

/// <summary>
/// 会话
/// </summary>
public interface IWebSocketSession
{
    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CloseAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <param name="closeStatus"></param>
    /// <param name="statusDescription"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken = default);
}