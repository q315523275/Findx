using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebSocketCore;

/// <summary>
///     连接管理
/// </summary>
public interface IWebSocketClientManager : IDisposable
{
    /// <summary>
    ///     连接总数
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     获取连接信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public WebSocketClient GetClient(string id);

    /// <summary>
    ///     获取所有连接集合
    /// </summary>
    /// <returns></returns>
    public IEnumerable<WebSocketClient> GetAllClients();

    /// <summary>
    ///     获取组中所有连接集合
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public IEnumerable<string> GetAllClientsFromGroup(string groupId);

    /// <summary>
    ///     添加连接信息
    /// </summary>
    /// <param name="clientInfo"></param>
    public void AddClient(WebSocketClient clientInfo);

    /// <summary>
    ///     添加连接组信息
    /// </summary>
    /// <param name="clientInfo"></param>
    /// <param name="groupId"></param>
    public void AddToGroup(WebSocketClient clientInfo, string groupId);

    /// <summary>
    ///     移除连接组信息
    /// </summary>
    /// <param name="clientInfo"></param>
    /// <param name="groupId"></param>
    public void RemoveFromGroup(WebSocketClient clientInfo, string groupId);

    /// <summary>
    ///     移除连接
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task RemoveClientAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     移除连接
    /// </summary>
    /// <param name="clientInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task RemoveClientAsync(WebSocketClient clientInfo, CancellationToken cancellationToken = default);
}