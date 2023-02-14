﻿using System;
using System.Linq;
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
        /// 连接管理器
        /// </summary>
        protected IWebSocketClientManager ClientManager { get; }

        /// <summary>
        /// 序列化工具
        /// </summary>
        private readonly IWebSocketSerializer _serializer;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="clientManager"></param>
        /// <param name="serializer"></param>
        protected WebSocketHandlerBase(IWebSocketClientManager clientManager, IWebSocketSerializer serializer)
        {
            ClientManager = clientManager;
            _serializer = serializer;
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="client"></param>
        public virtual async Task OnConnected(WebSocketClient client)
        {
            ClientManager.AddClient(client);

            await SendMessageAsync(client, new WebSocketMessage<string> { MessageType = MessageType.ConnectionEvent, Data = $"{client.Id},{client.Name},{client.RemoteIp},{client.ServerIp}" }).ConfigureAwait(false);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="client"></param>
        public virtual async Task OnDisconnected(WebSocketClient client)
        {
            await ClientManager.RemoveClientAsync(client).ConfigureAwait(false);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public async Task SendMessageAsync<T>(WebSocketClient client, WebSocketMessage<T> message)
        {
            if (client.Client.State != WebSocketState.Open)
                return;

            try
            {
                var content = _serializer.Serialize(message);
                var body = new ArraySegment<byte>(array: content, offset: 0, count: content.Length);
                
                await client.Client.SendAsync(buffer: body, messageType: WebSocketMessageType.Text, endOfMessage: true, cancellationToken: CancellationToken.None).ConfigureAwait(false);
            }
            catch (WebSocketException e)
            {
                if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    await OnDisconnected(client);
                }
            }
        }
        
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        public async Task SendMessageAsync<T>(string id, WebSocketMessage<T> message)
        {
            var client = ClientManager.GetClient(id);
            if (client != null)
                await SendMessageAsync(client, message).ConfigureAwait(false);
        }

        /// <summary>
        /// 给所有连接发送消息
        /// </summary>
        /// <param name="message"></param>
        public async Task SendMessageToAllAsync<T>(WebSocketMessage<T> message)
        {
            var allClients = ClientManager.GetAllClients();
            if (allClients != null)
            {
                foreach (var client in allClients)
                {
                    try
                    {
                        if (client.Client?.State == WebSocketState.Open)
                            await SendMessageAsync(client, message).ConfigureAwait(false);
                    }
                    catch (WebSocketException e)
                    {
                        if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                        {
                            await OnDisconnected(client);
                        }
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
            var clients = ClientManager.GetAllClientsFromGroup(groupId);
            if (clients != null)
            {
                foreach (var socket in clients)
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
        /// <param name="excepts">不发送id</param>
        public async Task SendMessageToGroupAsync<T>(string groupId, WebSocketMessage<T> message, params string[] excepts)
        {
            var clients = ClientManager.GetAllClientsFromGroup(groupId);
            if (clients != null)
            {
                foreach (var client in clients)
                {
                    if (!excepts.Contains(client))
                        await SendMessageAsync(client, message);
                }
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="result"></param>
        /// <param name="receivedMessage"></param>
        public virtual async Task ReceiveAsync(WebSocketClient client, WebSocketReceiveResult result, string receivedMessage)
        {
            try
            {
                await SendMessageAsync(client, new WebSocketMessage<string> { MessageType = MessageType.Text, Data = $"{client.Id},{client.Name},{client.RemoteIp},{client.ServerIp}:{receivedMessage}" }).ConfigureAwait(false);
            }
            catch (TargetParameterCountException)
            {
                await SendMessageAsync(client, new WebSocketMessage<string> { MessageType = MessageType.Error, Data = $"does not take parameters!" }).ConfigureAwait(false);
            }

            catch (ArgumentException)
            {
                await SendMessageAsync(client, new WebSocketMessage<string> { MessageType = MessageType.Error, Data = $"takes different arguments!" }).ConfigureAwait(false);
            }
        }
    }
}
