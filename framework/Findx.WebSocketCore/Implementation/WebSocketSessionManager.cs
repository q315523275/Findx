using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.WebSocketCore.Abstractions;

namespace Findx.WebSocketCore.Implementation;

/// <summary>
///     连接管理
/// </summary>
public class WebSocketSessionManager : IWebSocketSessionManager
{
    private readonly ConcurrentDictionary<string, List<IWebSocketSession>> _sessionDic = new();

    private readonly ConcurrentDictionary<string, List<string>> _groups = new();
    
    /// <summary>
    ///     释放资源
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async ValueTask DisposeAsync()
    {
        foreach (var item in _sessionDic)
        {
            await RemoveSessionAsync(item.Key);
        }
        _sessionDic.Clear();
        _groups.Clear();
    }

    /// <summary>
    ///     连接总数
    /// </summary>
    public int Count => _sessionDic.Count;

    /// <summary>
    ///     获取WebSocketClient信息
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public IEnumerable<IWebSocketSession> GetSession(string userName)
    {
        return _sessionDic.TryGetValue(userName, out var sessions) ? sessions : ArraySegment<IWebSocketSession>.Empty;
    }

    /// <summary>
    ///     获取所有客户端
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IWebSocketSession> GetAllSessions()
    {
        return _sessionDic.SelectMany(x => x.Value);
    }

    /// <summary>
    ///     获取组内全部连接id
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public IEnumerable<string> GetAllUserFromGroup(string groupId)
    {
        return _groups.TryGetValue(groupId, out var group) ? group : null;
    }

    /// <summary>
    ///     添加会话
    /// </summary>
    /// <param name="session"></param>
    public void AddSession(IWebSocketSession session)
    {
        if (!_sessionDic.ContainsKey(session.UserName))
        {
            _sessionDic[session.UserName] = [];
        }
        
        _sessionDic[session.UserName].Add(session);
    }

    /// <summary>
    ///     将会话添加至组内
    /// </summary>
    /// <param name="session"></param>
    /// <param name="groupId"></param>
    public void AddToGroup(IWebSocketSession session, string groupId)
    {
        if (_groups.TryGetValue(groupId, out var group) && group.All(x => x != session.UserName))
        {
            group.Add(session.UserName);
            return;
        }
        _groups.TryAdd(groupId, [session.UserName]);
    }

    /// <summary>
    ///     从组内删除指定会话
    /// </summary>
    /// <param name="session"></param>
    /// <param name="groupId"></param>
    public void RemoveFromGroup(IWebSocketSession session, string groupId)
    {
        if (_groups.TryGetValue(groupId, out var group)) group.Remove(session.UserName);
    }

    /// <summary>
    ///     移除用户所有会话
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="cancellationToken"></param>
    public async Task RemoveSessionAsync(string userName, CancellationToken cancellationToken = default)
    {
        if (userName.IsNullOrWhiteSpace()) return;
        
        if (_sessionDic.TryRemove(userName, out var sessions) && sessions != null && sessions.Any())
        {
            foreach (var session in sessions)
            {
                if (session.State == WebSocketState.Open)
                {
                    await session.CloseAsync(WebSocketCloseStatus.NormalClosure, "服务端主动关闭连接", cancellationToken).ConfigureAwait(false);
                }
                session.Dispose();
            }
        }
    }

    /// <summary>
    ///     移除session会话
    /// </summary>
    /// <param name="session"></param>
    /// <param name="cancellationToken"></param>
    public async Task RemoveSessionAsync(IWebSocketSession session, CancellationToken cancellationToken = default)
    {
        if (session.UserName.IsNullOrWhiteSpace()) return;
        
        if (_sessionDic.TryGetValue(session.UserName, out var sessions) && sessions != null && sessions.Any())
        {
            if (sessions.Any(x => x.Id == session.Id))
            {
                var client = sessions.FirstOrDefault(x => x.Id == session.Id);
                if (session.State == WebSocketState.Open)
                {
                    await session.CloseAsync(WebSocketCloseStatus.NormalClosure, "服务端主动关闭连接", cancellationToken).ConfigureAwait(false);
                }
                session.Dispose();
                
                sessions.Remove(client);
            }
            
            if (!sessions.Any()) _sessionDic.TryRemove(session.UserName, out _);
        }
    }
}