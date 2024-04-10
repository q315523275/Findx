using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Microsoft.Extensions.Logging;

namespace Findx.WebSocketCore;

/// <summary>
///     自定义WebSocket客户端
/// </summary>
public class XWebSocketClient : IWebSocketSession, IDisposable
{
    private CancellationTokenSource _cts;

    private readonly ILogger _logger;

    private ClientWebSocket _clientWebSocket;

    /// <summary>
    ///     WebSocket服务端地址
    /// </summary>
    /// <param name="wsUrl"></param>
    /// <param name="logger"></param>
    public XWebSocketClient(string wsUrl, ILogger logger = null)
    {
        Check.NotNull(wsUrl, nameof(wsUrl));

        Headers = new Dictionary<string, string>();
        Uri = new Uri(wsUrl);

        _logger = logger;
        _clientWebSocket = new ClientWebSocket();
    }

    /// <summary>
    ///     Uri地址
    /// </summary>
    public Uri Uri { get; }

    /// <summary>
    ///     状态
    /// </summary>
    public XWebSocketState Status { set; get; }

    /// <summary>
    ///     请求头字典
    /// </summary>
    public IDictionary<string, string> Headers { set; get; }

    /// <summary>
    ///     消息接收委托方法
    /// </summary>
    public Func<IWebSocketSession, string, CancellationToken, Task> MessageReceived { set; get; }
    
    /// <summary>
    ///     异常委托方法
    /// </summary>
    public Func<string, Exception, Task> OnException { set; get; }

    /// <summary>
    ///     关闭委托方法
    /// </summary>
    public Func<Task> OnClosed { set; get; }

    
    /// <summary>
    ///     开始
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> StartAsync(CancellationToken cancellationToken = default)
    {
        await ConnectAsync(Uri, Headers, cancellationToken);
        
        _cts = new CancellationTokenSource();
        
        ReceiveMessageAsync();

        Status = XWebSocketState.Open;

        return true;
    }

    /// <summary>
    ///     发送消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SendAsync(byte[] message, CancellationToken cancellationToken = default)
    {
        // 发送消息
        await _clientWebSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     发送消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        var body = Encoding.UTF8.GetBytes(message);
        // 发送消息
        await _clientWebSocket.SendAsync(new ArraySegment<byte>(body), WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     关闭连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        await CloseAsync(WebSocketCloseStatus.NormalClosure, "主动关闭连接", cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     关闭连接
    /// </summary>
    /// <param name="closeStatus"></param>
    /// <param name="statusDescription"></param>
    /// <param name="cancellationToken"></param>
    public async Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken = default)
    {
        await _clientWebSocket.CloseAsync(closeStatus, statusDescription, cancellationToken).ConfigureAwait(false);
        Status = XWebSocketState.Closed;
        // ReSharper disable once PossibleNullReferenceException
        await OnClosed?.Invoke();
    }

    
    /// <summary>
    ///     连接
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="headers"></param>
    /// <param name="cancellationToken"></param>
    private async Task ConnectAsync(Uri uri, IDictionary<string, string> headers, CancellationToken cancellationToken = default)
    {
        if (Status is XWebSocketState.Open or XWebSocketState.Connecting || _clientWebSocket?.State == WebSocketState.Open) 
            return;

        _clientWebSocket?.Abort();
        _clientWebSocket?.Dispose();
        _clientWebSocket = null;

        Status = XWebSocketState.Closed;

        if (_clientWebSocket == null)
        {
            Status = XWebSocketState.Connecting;
            _clientWebSocket = new ClientWebSocket();
        }

        foreach (var kv in headers) _clientWebSocket.Options.SetRequestHeader(kv.Key, kv.Value);

        _logger?.LogTrace("client try connect to server {Uri}", uri);

        await _clientWebSocket.ConnectAsync(uri, cancellationToken);

        if (_clientWebSocket.State == WebSocketState.Open)
            _logger?.LogTrace("client connect server {Uri} successful ", uri);
    }
    
    /// <summary>
    ///     接收消息
    /// </summary>
    private void ReceiveMessageAsync()
    {
        Task.Factory.StartNew(async () =>
        {
            // 缓冲区大小
            var buffer = new byte[2048];
            
            // ReSharper disable once PossibleNullReferenceException
            while (_clientWebSocket.State == WebSocketState.Open)
            {
                var arraySegment = new ArraySegment<byte>(buffer);
                string message = null;
                try
                {
                    WebSocketReceiveResult result;
                    using (var ms = Pool.MemoryStream.Rent())
                    {
                        do
                        {
                            result = await _clientWebSocket.ReceiveAsync(arraySegment, _cts.Token).ConfigureAwait(false);
                            if (arraySegment.Array != null)
                                ms.Write(arraySegment.Array, arraySegment.Offset, result.Count);
                            
                        } while (!result.EndOfMessage);

                        ms.Seek(0, SeekOrigin.Begin);
                        using (var reader = new StreamReader(ms, Encoding.UTF8))
                        {
                            message = await reader.ReadToEndAsync().ConfigureAwait(false);
                        }
                    }

                    HandleMessageAsync(result, message, _cts.Token);
                }
                catch (WebSocketException ex)
                {
                    if (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {
                        Dispose();
                        Status = XWebSocketState.Closed;
                    }
                    
                    if (OnException != null) await OnException(message, ex);
                }
                catch (Exception ex)
                {
                    if (OnException != null) await OnException(message, ex);
                }
            }
            
        }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
    }

    /// <summary>
    ///     处理消息
    /// </summary>
    /// <param name="result"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    private async Task HandleMessageAsync(WebSocketReceiveResult result, string message, CancellationToken cancellationToken = default)
    {
        if (result.MessageType == WebSocketMessageType.Text && MessageReceived != null)
        {
            await MessageReceived.Invoke(this, message, cancellationToken).ConfigureAwait(false);
        }

        if (result.MessageType == WebSocketMessageType.Close)
        {
            Dispose();
            Status = XWebSocketState.Closed;
            // ReSharper disable once PossibleNullReferenceException
            await OnClosed?.Invoke();
        }
    }
    
    /// <summary>
    ///     Dispose
    /// </summary>
    public void Dispose()
    {
        _clientWebSocket?.Abort();
        _clientWebSocket?.Dispose();
        _clientWebSocket = null;
        _cts?.Cancel();
    }
}