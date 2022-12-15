using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebSocketCore
{
    /// <summary>
    /// 连接管理器
    /// </summary>
    public class WebSocketConnectionManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

        private readonly ConcurrentDictionary<string, List<string>> _groups = new();

        /// <summary>
        /// 根据连接id查询连接
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WebSocket GetSocketById(string id)
        {
            return _sockets.TryGetValue(id, out var socket) ? socket : null;
        }

        /// <summary>
        /// 查询所有连接
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<string, WebSocket> GetAll()
        {
            return _sockets;
        }

        /// <summary>
        /// 获取小组内的全部连接id
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public List<string> GetAllFromGroup(string groupId)
        {
            return _groups.ContainsKey(groupId) ? _groups[groupId] : default;
        }

        /// <summary>
        /// 根据连接获取id
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public string GetConnectionId(WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value == socket).Key;
        }

        /// <summary>
        /// 添加连接
        /// </summary>
        /// <param name="socket"></param>
        public void AddSocket(WebSocket socket)
        {
            _sockets.TryAdd(CreateConnectionId(), socket);
        }

        /// <summary>
        /// /添加连接
        /// </summary>
        /// <param name="socketId"></param>
        /// <param name="socket"></param>
        public void AddSocket(string socketId, WebSocket socket)
        {
            _sockets.TryAdd(socketId, socket);
        }

        /// <summary>
        /// 加入组
        /// </summary>
        /// <param name="socketId"></param>
        /// <param name="groupId"></param>
        public void AddToGroup(string socketId, string groupId)
        {
            if (_groups.ContainsKey(groupId))
            {
                _groups[groupId].Add(socketId);

                return;
            }

            _groups.TryAdd(groupId, new List<string> { socketId });
        }

        /// <summary>
        /// 退出组
        /// </summary>
        /// <param name="socketId"></param>
        /// <param name="groupId"></param>
        public void RemoveFromGroup(string socketId, string groupId)
        {
            if (_groups.ContainsKey(groupId))
            {
                _groups[groupId].Remove(socketId);
            }
        }

        /// <summary>
        /// 移除连接
        /// </summary>
        /// <param name="id"></param>
        public async Task RemoveSocket(string id)
        {
            if (id == null) return;

            if (_sockets.TryRemove(id, out var socket))
            {
                if (socket.State != WebSocketState.Open) return;

                await socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                        statusDescription: "Closed by the WebSocketManager",
                                        cancellationToken: CancellationToken.None).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 创建连接id
        /// </summary>
        /// <returns></returns>
        private static string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 查看连接总数
        /// </summary>
        /// <returns></returns>
        public int GetSocketClientCount()
        {
            return _sockets.Count;
        }

        /// <summary>
        /// 释放资源，关闭连接
        /// </summary>
        public void Dispose()
        {
            foreach (var item in _sockets.Values)
            {
                item?.Dispose();
            }
            _sockets.Clear();
        }
    }
}
