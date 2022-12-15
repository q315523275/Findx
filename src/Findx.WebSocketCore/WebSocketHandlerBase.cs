using System;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebSocketCore
{
    /// <summary>
    /// 处理器基类
    /// </summary>
    public abstract class WebSocketHandlerBase
    {
        /// <summary>
        /// 管理器
        /// </summary>
        private WebSocketConnectionManager WebSocketConnectionManager { get; }

        /// <summary>
        /// 序列化工具
        /// </summary>
        private readonly IWebSocketSerializer _serializer;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="webSocketConnectionManager"></param>
        /// <param name="serializer"></param>
        protected WebSocketHandlerBase(WebSocketConnectionManager webSocketConnectionManager, IWebSocketSerializer serializer)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
            _serializer = serializer;
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="socket"></param>
        public virtual async Task OnConnected(WebSocket socket)
        {
            WebSocketConnectionManager.AddSocket(socket);

            await SendMessageAsync(socket, new WebSocketMessage<string>
            {
                MessageType = MessageType.ConnectionEvent,
                Data = WebSocketConnectionManager.GetConnectionId(socket)
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="socketId"></param>
        /// <param name="socket"></param>
        public virtual async Task OnConnected(string socketId, WebSocket socket)
        {
            WebSocketConnectionManager.AddSocket(socketId, socket);

            await SendMessageAsync(socket, new WebSocketMessage<string>
            {
                MessageType = MessageType.ConnectionEvent,
                Data = WebSocketConnectionManager.GetConnectionId(socket)
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="socket"></param>
        public virtual async Task OnDisconnected(WebSocket socket)
        {
            var socketId = WebSocketConnectionManager.GetConnectionId(socket);
            if (!string.IsNullOrWhiteSpace(socketId))
                await WebSocketConnectionManager.RemoveSocket(socketId).ConfigureAwait(false);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        public async Task SendMessageAsync<T>(WebSocket socket, WebSocketMessage<T> message)
        {
            if (socket.State != WebSocketState.Open)
                return;
            
            var body = _serializer.Serialize(message);
            
            try
            {
                await socket.SendAsync(buffer: new ArraySegment<byte>(array: body,
                                                                      offset: 0,
                                                                      count: body.Length),
                                       messageType: WebSocketMessageType.Text,
                                       endOfMessage: true,
                                       cancellationToken: CancellationToken.None).ConfigureAwait(false);
            }
            catch (WebSocketException e)
            {
                if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    await OnDisconnected(socket);
                }
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="socketId"></param>
        /// <param name="message"></param>
        public async Task SendMessageAsync<T>(string socketId, WebSocketMessage<T> message)
        {
            var socket = WebSocketConnectionManager.GetSocketById(socketId);
            if (socket != null)
                await SendMessageAsync(socket, message).ConfigureAwait(false);
        }

        /// <summary>
        /// 给所有连接发送消息
        /// </summary>
        /// <param name="message"></param>
        public async Task SendMessageToAllAsync<T>(WebSocketMessage<T> message)
        {
            foreach (var pair in WebSocketConnectionManager.GetAll())
            {
                try
                {
                    if (pair.Value.State == WebSocketState.Open)
                        await SendMessageAsync(pair.Value, message).ConfigureAwait(false);
                }
                catch (WebSocketException e)
                {
                    if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {
                        await OnDisconnected(pair.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 给组成员发送消息
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="message"></param>
        public async Task SendMessageToGroupAsync<T>(string groupId, WebSocketMessage<T> message)
        {
            var sockets = WebSocketConnectionManager.GetAllFromGroup(groupId);
            if (sockets != null)
            {
                foreach (var socket in sockets)
                {
                    await SendMessageAsync(socket, message);
                }
            }
        }

        /// <summary>
        /// 给组成员发送消息
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="message"></param>
        /// <param name="except">不发送id</param>
        public async Task SendMessageToGroupAsync<T>(string groupId, WebSocketMessage<T> message, string except)
        {
            var sockets = WebSocketConnectionManager.GetAllFromGroup(groupId);
            if (sockets != null)
            {
                foreach (var id in sockets)
                {
                    if (id != except)
                        await SendMessageAsync(id, message);
                }
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="result"></param>
        /// <param name="receivedMessage"></param>
        public virtual async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, WebSocketMessage receivedMessage)
        {
            try
            {
                await SendMessageAsync(socket, new WebSocketMessage<string> { MessageType = MessageType.Text, Data = $"receive content:{receivedMessage.Data}" }).ConfigureAwait(false);
            }
            catch (TargetParameterCountException)
            {
                await SendMessageAsync(socket, new WebSocketMessage<string> { MessageType = MessageType.Error, Data = $"does not take parameters!" }).ConfigureAwait(false);
            }

            catch (ArgumentException)
            {
                await SendMessageAsync(socket, new WebSocketMessage<string> { MessageType = MessageType.Error, Data = $"takes different arguments!" }).ConfigureAwait(false);
            }
        }
    }
}
