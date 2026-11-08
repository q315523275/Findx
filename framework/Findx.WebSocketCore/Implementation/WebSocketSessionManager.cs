using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.WebSocketCore.Abstractions;

namespace Findx.WebSocketCore.Implementation;

/// <summary>
///     连接管理
/// </summary>
/// <remarks>
///     外层按用户名分组，内层按会话Id精确维护，全程并发安全：
///     外层 ConcurrentDictionary 保证用户名级原子存取；内层 ConcurrentDictionary 保证同用户多会话的增删与枚举快照安全。
/// </remarks>
public class WebSocketSessionManager : IWebSocketSessionManager
{
    /// <summary>
    ///     用户名 -> (会话Id -> 会话)
    /// </summary>
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, IWebSocketSession>> _sessionDic = new();

    /// <summary>
    ///     组Id -> (用户名 -> 占位标记)
    /// </summary>
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _groups = new();

    /// <summary>
    ///     释放资源
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        //  先拍快照再逐个移除，避免遍历期间字典被并发修改
        foreach (var userName in _sessionDic.Keys.ToArray())
        {
            await RemoveSessionAsync(userName).ConfigureAwait(false);
        }
        _sessionDic.Clear();
        _groups.Clear();
    }

    /// <summary>
    ///     连接总数（按用户名计）
    /// </summary>
    public int Count => _sessionDic.Count;

    /// <summary>
    ///     获取WebSocketClient信息
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public IEnumerable<IWebSocketSession> GetSession(string userName)
    {
        return _sessionDic.TryGetValue(userName, out var sessions) ? sessions.Values.ToArray() : Array.Empty<IWebSocketSession>();
    }

    /// <summary>
    ///     获取所有客户端（快照）
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IWebSocketSession> GetAllSessions()
    {
        return _sessionDic.SelectMany(x => x.Value.Values).ToArray();
    }

    /// <summary>
    ///     获取组内全部连接id
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public IEnumerable<string> GetAllUserFromGroup(string groupId)
    {
        return _groups.TryGetValue(groupId, out var group) ? group.Keys.ToArray() : null;
    }

    /// <summary>
    ///     添加会话
    /// </summary>
    /// <param name="session"></param>
    public void AddSession(IWebSocketSession session)
    {
        //  GetOrAdd 原子建桶，AddOrUpdate 按会话Id登记，杜绝 check-then-act 竞态
        _sessionDic.GetOrAdd(session.UserName, _ => new ConcurrentDictionary<long, IWebSocketSession>())
            .AddOrUpdate(session.Id, session, (_, _) => session);
    }

    /// <summary>
    ///     将会话添加至组内
    /// </summary>
    /// <param name="session"></param>
    /// <param name="groupId"></param>
    public void AddToGroup(IWebSocketSession session, string groupId)
    {
        _groups.GetOrAdd(groupId, _ => new ConcurrentDictionary<string, byte>())
            .TryAdd(session.UserName, 0);
    }

    /// <summary>
    ///     从组内删除指定会话
    /// </summary>
    /// <param name="session"></param>
    /// <param name="groupId"></param>
    public void RemoveFromGroup(IWebSocketSession session, string groupId)
    {
        if (_groups.TryGetValue(groupId, out var group))
        {
            group.TryRemove(session.UserName, out _);

            //  组内无人时清理空组key，防止空组key无限累积
            if (group.IsEmpty) _groups.TryRemove(groupId, out _);
        }
    }

    /// <summary>
    ///     移除用户所有会话
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="cancellationToken"></param>
    public async Task RemoveSessionAsync(string userName, CancellationToken cancellationToken = default)
    {
        if (userName.IsNullOrWhiteSpace()) return;

        if (_sessionDic.TryRemove(userName, out var sessions) && sessions != null)
        {
            foreach (var session in sessions.Values)
            {
                await session.DisposeAsync().ConfigureAwait(false);
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

        if (_sessionDic.TryGetValue(session.UserName, out var sessions) && sessions != null)
        {
            //  按会话Id精确移除目标session
            if (sessions.TryRemove(session.Id, out var client) && client != null)
            {
                await client.DisposeAsync().ConfigureAwait(false);
            }

            //  判断是否清除客户连接
            if (sessions.IsEmpty) _sessionDic.TryRemove(session.UserName, out _);
        }
    }
}
