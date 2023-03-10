namespace Findx.WebSocketCore;

/// <summary>
/// WebSocket状态枚举
/// </summary>
public enum XWebSocketState
{
    /// <summary>
    /// None
    /// </summary>
    None,
    
    /// <summary>
    /// 连接中
    /// </summary>
    Connecting,
    
    /// <summary>
    /// 已连接
    /// </summary>
    Open,

    /// <summary>
    /// 已关闭
    /// </summary>
    Closed
}