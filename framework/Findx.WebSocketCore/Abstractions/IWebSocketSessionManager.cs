using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebSocketCore.Abstractions;

/// <summary>
///     WebSocket session 管理器
/// </summary>
public interface IWebSocketSessionManager
{
    /// <summary>
    ///     连接总数
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     获取用户连接信息
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public IEnumerable<IWebSocketSession> GetSession(string userName);

    /// <summary>
    ///     获取所有连接集合
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IWebSocketSession> GetAllSessions();

    /// <summary>
    ///     获取组中所有用户集合
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public IEnumerable<string> GetAllUserFromGroup(string groupId);

    /// <summary>
    ///     添加用户连接会话
    /// </summary>
    /// <param name="session"></param>
    public void AddSession(IWebSocketSession session);

    /// <summary>
    ///     添加连接组信息
    /// </summary>
    /// <param name="session"></param>
    /// <param name="groupId"></param>
    public void AddToGroup(IWebSocketSession session, string groupId);

    /// <summary>
    ///     移除连接组信息
    /// </summary>
    /// <param name="session"></param>
    /// <param name="groupId"></param>
    public void RemoveFromGroup(IWebSocketSession session, string groupId);

    /// <summary>
    ///     移除用户所有连接会话
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task RemoveSessionAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     移除连接会话
    /// </summary>
    /// <param name="session"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task RemoveSessionAsync(IWebSocketSession session, CancellationToken cancellationToken = default);
}
