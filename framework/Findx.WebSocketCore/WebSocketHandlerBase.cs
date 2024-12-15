using System;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.WebSocketCore.Abstractions;

namespace Findx.WebSocketCore;

/// <summary>
///     处理器基类
/// </summary>
public abstract class WebSocketHandlerBase
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="webSocketSessionManager"></param>
    protected WebSocketHandlerBase(IWebSocketSessionManager webSocketSessionManager)
    {
        WebSocketSessionManager = webSocketSessionManager;
    }

    /// <summary>
    ///     连接管理器
    /// </summary>
    protected IWebSocketSessionManager WebSocketSessionManager { get; }

    /// <summary>
    ///     连接成功
    /// </summary>
    /// <param name="session"></param>
    /// <param name="cancellationToken"></param>
    public virtual async Task OnConnected(IWebSocketSession session, CancellationToken cancellationToken = default)
    {
        WebSocketSessionManager.AddSession(session);

        await SendMessageAsync(session, new RequestTextMessage($"user:{session.UserName} session:{session.Id} from {session.RemoteIpAddress}:{session.RemotePort}"), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     关闭连接
    /// </summary>
    /// <param name="session"></param>
    /// <param name="cancellationToken"></param>
    public virtual async Task OnDisconnected(IWebSocketSession session, CancellationToken cancellationToken = default)
    {
        await WebSocketSessionManager.RemoveSessionAsync(session, cancellationToken).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     发送消息
    /// </summary>
    /// <param name="session"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ArgumentException"></exception>
    public async Task SendMessageAsync(IWebSocketSession session, RequestMessage message, CancellationToken cancellationToken = default)
    {
        ReadOnlyMemory<byte> payload;

        switch (message)
        {
            case RequestTextMessage textMessage:
                payload = MemoryMarshal.AsMemory<byte>(textMessage.Text.ToBytes());
                break;
            case RequestBinaryMessage binaryMessage:
                payload = MemoryMarshal.AsMemory<byte>(binaryMessage.Data);
                break;
            case RequestBinarySegmentMessage segmentMessage:
                payload = segmentMessage.Data.AsMemory();
                break;
            default:
                throw new ArgumentException($"Unknown message type: {message.GetType()}");
        }
        
        await session.SendAsync(payload, WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     发送消息
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ArgumentException"></exception>
    public async Task SendMessageAsync(string userName, RequestMessage message, CancellationToken cancellationToken = default)
    {
        var sessions = WebSocketSessionManager.GetSession(userName);
        foreach (var session in sessions)
        {
            await SendMessageAsync(session, message, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     给所有连接发送消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    public async Task SendMessageToAllAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        var allSessions = WebSocketSessionManager.GetAllSessions();
        if (allSessions != null)
        {
            foreach (var session in allSessions)
            {
                try
                {
                    if (session?.State == WebSocketState.Open)
                        await SendMessageAsync(session, message, cancellationToken).ConfigureAwait(false);
                }
                catch (WebSocketException e)
                {
                    if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely) await OnDisconnected(session, cancellationToken);
                }
            }
        }
    }

    /// <summary>
    ///     给组成员发送消息
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    public async Task SendMessageToGroupAsync(string groupId, RequestMessage message, CancellationToken cancellationToken = default)
    {
        var users = WebSocketSessionManager.GetAllUserFromGroup(groupId);
        if (users != null)
        {
            foreach (var user in users)
            {
                await SendMessageAsync(user, message, cancellationToken);
            }
        }
    }

    /// <summary>
    ///     给组成员发送消息
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="message"></param>
    /// <param name="excepts">不发送id</param>
    public async Task SendMessageToGroupAsync(string groupId, RequestMessage message, params string[] excepts)
    {
        var users = WebSocketSessionManager.GetAllUserFromGroup(groupId);
        if (users != null)
        {
            foreach (var user in users)
            {
                if (!excepts.Contains(user)) await SendMessageAsync(user, message);
            }
        }
    }

    /// <summary>
    ///     接收消息
    /// </summary>
    /// <param name="client"></param>
    /// <param name="message"></param>
    /// <param name="result"></param>
    /// <param name="cancellationToken"></param>
    public abstract Task ReceiveAsync(IWebSocketSession client, ResponseMessage message, ValueWebSocketReceiveResult result, CancellationToken cancellationToken = default);
}