using Findx.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebSocketCore
{
    /// <summary>
    /// 中间件
    /// </summary>
    public class WebSocketManagerMiddleware
    {
        private readonly RequestDelegate _next;
        
        private WebSocketHandlerBase WebSocketHandler { get; }
        
        private IWebSocketSerializer Serializer { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="webSocketHandler"></param>
        /// <param name="serializer"></param>
        public WebSocketManagerMiddleware(RequestDelegate next, WebSocketHandlerBase webSocketHandler, IWebSocketSerializer serializer)
        {
            _next = next;
            WebSocketHandler = webSocketHandler;
            Serializer = serializer;
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }

            var query = context.Request.QueryString.Value;
            var socket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(query))
            {
                await WebSocketHandler.OnConnected(socket).ConfigureAwait(false);
            }
            else
            {
                query = query.TrimStart('?');
                await WebSocketHandler.OnConnected(query, socket).ConfigureAwait(false);
            }

            await ReceiveAsync(socket, async (result, serializedMessage) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Serializer.Deserialize<WebSocketMessage>(serializedMessage);
                    await WebSocketHandler.ReceiveAsync(socket, result, message).ConfigureAwait(false);
                    return;
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await WebSocketHandler.OnDisconnected(socket);
                }
            });
        }

        private async Task ReceiveAsync(WebSocket socket, Func<WebSocketReceiveResult, byte[], Task> handleMessage)
        {
            while (socket.State == WebSocketState.Open)
            {
                var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
                try
                {
                    byte[] message;
                    WebSocketReceiveResult result;
                    using (var ms = new MemoryStream())
                    {
                        do
                        {
                            result = await socket.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                            ms.Write(buffer.Array, buffer.Offset, result.Count);
                        }
                        while (!result.EndOfMessage);

                        ms.Seek(0, SeekOrigin.Begin);

                        message = ms.ToArray();
                        
                        // using (var reader = new StreamReader(ms, Encoding.UTF8))
                        // {
                        //     message = await reader.ReadToEndAsync().ConfigureAwait(false);
                        // }
                    }

                    handleMessage(result, message);
                }
                catch (WebSocketException e)
                {
                    if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {
                        socket.Abort();
                    }
                }
            }

            await WebSocketHandler.OnDisconnected(socket);
        }
    }
}
