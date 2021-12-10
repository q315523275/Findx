using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebSocketCore
{
    public class WebSocketConnectionManager : IDisposable
    {
        private ConcurrentDictionary<string, WebSocket> _sockets = new();
        private ConcurrentDictionary<string, List<string>> _groups = new();

        public WebSocket GetSocketById(string id)
        {
            if (_sockets.TryGetValue(id, out WebSocket socket))
                return socket;
            else
                return null;
        }

        public ConcurrentDictionary<string, WebSocket> GetAll()
        {
            return _sockets;
        }

        public List<string> GetAllFromGroup(string GroupID)
        {
            if (_groups.ContainsKey(GroupID))
            {
                return _groups[GroupID];
            }

            return default;
        }

        public string GetId(WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value == socket).Key;
        }

        public void AddSocket(WebSocket socket)
        {
            _sockets.TryAdd(CreateConnectionId(), socket);
        }

        public void AddSocket(string socketID, WebSocket socket)
        {
            _sockets.TryAdd(socketID, socket);
        }

        public void AddToGroup(string socketID, string groupID)
        {
            if (_groups.ContainsKey(groupID))
            {
                _groups[groupID].Add(socketID);

                return;
            }

            _groups.TryAdd(groupID, new List<string> { socketID });
        }

        public void RemoveFromGroup(string socketID, string groupID)
        {
            if (_groups.ContainsKey(groupID))
            {
                _groups[groupID].Remove(socketID);
            }
        }

        public async Task RemoveSocket(string id)
        {
            if (id == null) return;

            if (_sockets.TryRemove(id, out WebSocket socket))
            {
                if (socket.State != WebSocketState.Open) return;

                await socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                        statusDescription: "Closed by the WebSocketManager",
                                        cancellationToken: CancellationToken.None).ConfigureAwait(false);
            }
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }

        public int GetSocketClientCount()
        {
            return _sockets.Count();
        }

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
