using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebSocketCore;

/// <summary>
/// 连接管理
/// </summary>
public class WebSocketClientManager : IWebSocketClientManager
{
    private readonly ConcurrentDictionary<string, WebSocketClient> _clients = new();

    private readonly ConcurrentDictionary<string, List<string>> _groups = new();
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        foreach (var item in _clients.Values)
        {
            item?.Client?.Abort();
            item?.Client?.Dispose();
        }
        _clients.Clear();
        _groups.Clear();
    }

    /// <summary>
    /// 连接总数
    /// </summary>
    public int Count => _clients.Count();

    /// <summary>
    /// 获取WebSocketClient信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public WebSocketClient GetClient(string id)
    {
        return _clients.TryGetValue(id, out var client) ? client : null;
    }

    /// <summary>
    /// 获取全部连接信息
    /// </summary>
    /// <returns></returns>
    public IEnumerable<WebSocketClient> GetAllClients()
    {
        return _clients.Select(item => item.Value);
    }

    /// <summary>
    /// 获取组内全部连接id
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public IEnumerable<string> GetAllClientsFromGroup(string groupId)
    {
        return _groups.ContainsKey(groupId) ? _groups[groupId] : default;
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="clientInfo"></param>
    public void AddClient(WebSocketClient clientInfo)
    {
        _clients.TryAdd(clientInfo.Id, clientInfo);
    }

    /// <summary>
    /// 添加组
    /// </summary>
    /// <param name="clientInfo"></param>
    /// <param name="groupId"></param>
    public void AddToGroup(WebSocketClient clientInfo, string groupId)
    {
        if (_groups.ContainsKey(groupId))
        {
            _groups[groupId].Add(clientInfo.Id);

            return;
        }

        _groups.TryAdd(groupId, new List<string> { clientInfo.Id });
    }

    /// <summary>
    /// 退出组
    /// </summary>
    /// <param name="clientInfo"></param>
    /// <param name="groupId"></param>
    public void RemoveFromGroup(WebSocketClient clientInfo, string groupId)
    {
        if (_groups.ContainsKey(groupId))
        {
            _groups[groupId].Remove(clientInfo.Id);
        }
    }

    /// <summary>
    /// 移除连接
    /// </summary>
    /// <param name="id"></param>
    public async Task RemoveClientAsync(string id)
    {
        if (id == null) return;

        if (_clients.TryRemove(id, out var client))
        {
            if (client.Client?.State != WebSocketState.Open) 
                return;

            await client.Client.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                           statusDescription: "Closed by the WebSocketManager",
                                           cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 移除连接
    /// </summary>
    /// <param name="clientInfo"></param>
    public async Task RemoveClientAsync(WebSocketClient clientInfo)
    {
        if (clientInfo?.Id == null) return;

        if (_clients.TryRemove(clientInfo.Id, out var client))
        {
            if (client.Client?.State != WebSocketState.Open) 
                return;

            await client.Client.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                           statusDescription: "Closed by the WebSocketManager",
                                           cancellationToken: CancellationToken.None).ConfigureAwait(false);
            
            client.Client?.Abort();
            client.Client?.Dispose();
            client.Client = default;
        }
    }
}