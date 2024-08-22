using System.Net.WebSockets;

namespace Findx.WebSocketCore.Hubs.Server;

/// <summary>
///     连接客户端
/// </summary>
public interface IXClient
{
    /// <summary>
    ///     WebSocket
    /// </summary>
    WebSocket WebSocket { get; }
    
    /// <summary>
    ///     用户账号
    /// </summary>
    string UserName { get; }
}