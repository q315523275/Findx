using System;
using System.Net.WebSockets;
using System.Text.Json.Serialization;

namespace Findx.WebSocketCore;

/// <summary>
///     连接信息
/// </summary>
public class XClientInfo
{
    /// <summary>
    ///     编号
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     连接Ip
    /// </summary>
    public string RemoteIp { get; set; }

    /// <summary>
    ///     服务Ip
    /// </summary>
    public string ServerIp { get; set; }

    /// <summary>
    ///     标签
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    ///     环境变量
    /// </summary>
    public string Environment { get; set; }

    /// <summary>
    ///     最后心跳时间
    /// </summary>
    public DateTime LastHeartbeatTime { get; set; }
}

/// <summary>
///     WebSocket连接信息
/// </summary>
public class WebSocketXClient : XClientInfo
{
    /// <summary>
    ///     连接端
    /// </summary>
    [JsonIgnore]
    public WebSocket Client { get; set; }
}