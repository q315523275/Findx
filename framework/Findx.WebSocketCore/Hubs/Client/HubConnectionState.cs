namespace Findx.WebSocketCore.Hubs.Client;

/// <summary>
///     连接状态
/// </summary>
public enum HubConnectionState
{
    /// <summary>The hub connection is disconnected.</summary>
    Disconnected,
    /// <summary>The hub connection is connected.</summary>
    Connected,
    /// <summary>The hub connection is connecting.</summary>
    Connecting,
    /// <summary>The hub connection is reconnecting.</summary>
    Reconnecting
}