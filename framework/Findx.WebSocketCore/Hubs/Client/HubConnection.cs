using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Locks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace Findx.WebSocketCore.Hubs.Client;

/// <summary>
///     通信连接类
/// </summary>
public class HubConnection: IAsyncDisposable
{
    private static readonly RecyclableMemoryStreamManager MemoryStreamPool = new();
    
    private readonly AsyncLock _lock = new();

    private readonly CancellationTokenSource _cts;
    
    private readonly TimeSpan _reconnectInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="url"></param>
    /// <param name="automaticReconnect">释放自动重连</param>
    public HubConnection(string url, bool automaticReconnect)
    {
        _cts = new CancellationTokenSource();
        
        _uri = new Uri(url);
        
        if (automaticReconnect) HandleReconnect();
    }

    /// <summary>
    ///     通信状态
    /// </summary>
    public HubConnectionState State { get; private set; }

    /// <summary>
    ///     通信关闭委托
    /// </summary>
    public event Func<Exception, Task> Closed;

    /// <summary>
    ///     通信尝试重连委托
    /// </summary>
    public event Func<Task> Reconnecting;
    
    /// <summary>
    ///     通信重连成功委托
    /// </summary>
    public event Func<Task> Reconnected;
    
    /// <summary>
    ///     通信地址
    /// </summary>
    private readonly Uri _uri;
    
    /// <summary>
    ///     使用 ClientWebSocket 通信
    /// </summary>
    protected ClientWebSocket ClientWebSocket;
    
    /// <summary>
    ///     header字典信息
    /// </summary>
    private IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
    
    /// <summary>
    ///     设置请求头
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public virtual void SetRequestHeader(string key, string value)
    {
        Headers.TryAdd(key, value);
    }
    
    /// <summary>
    ///     开始连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    public virtual async Task StartAsync(CancellationToken cancellationToken = default)
    {
        using var asyncLock = await _lock.LockAsync(cancellationToken);
        
        if (ClientWebSocket is { State: WebSocketState.Open })
        {
            State = HubConnectionState.Connected;
            return;
        }
        
        State = HubConnectionState.Connecting;

        ClientWebSocket = new ClientWebSocket();

        foreach (var (key, value) in Headers)
        {
            ClientWebSocket.Options.SetRequestHeader(key, value);
        }
        
        await ConnectInnerAsync(cancellationToken);

        State = ClientWebSocket.State == WebSocketState.Open ? HubConnectionState.Connected : HubConnectionState.Disconnected;
    }

    /// <summary>
    ///     关闭连接
    /// </summary>
    /// <param name="statusDescription"></param>
    /// <param name="cancellationToken"></param>
    public virtual async Task StopAsync(string statusDescription = "客户端主动停止...", CancellationToken cancellationToken = default)
    {
        using var asyncLock = await _lock.LockAsync(cancellationToken);
        
        if (ClientWebSocket is { State: WebSocketState.Open })
        {
            await ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, statusDescription, cancellationToken);

            State = HubConnectionState.Disconnected;
            
            if (Closed != null) await Closed(null);
        }
    }
    
    /// <summary>
    ///     发送消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="messageType"></param>
    /// <param name="endOfMessage"></param>
    /// <param name="cancellationToken"></param>
    public virtual async Task SendAsync(RequestMessage message, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken = default)
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
        
        await ClientWebSocket.SendAsync(payload, messageType, endOfMessage, cancellationToken);
    }
    
    /// <summary>
    ///     消息接收委托方法
    /// </summary>
    private Func<ResponseMessage, CancellationToken, Task> MessageReceived { set; get; }
    
    /// <summary>
    ///     消息事件
    /// </summary>
    /// <param name="received"></param>
    public void On(Func<ResponseMessage, CancellationToken, Task> received)
    {
        MessageReceived += received;
        StartListening();
    }

    #region Listen

    private readonly AtomicInteger _receivedCounter = new();
    
    /// <summary>
    ///     接收消息
    /// </summary>
    private void StartListening()
    {
        if (_receivedCounter.IncrementAndGet() != 1) return;
        
        _ = Task.Factory.StartNew(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                if (ClientWebSocket.State != WebSocketState.Open)
                {
                    await Task.Delay(_reconnectInterval);
                    continue;
                }

                // 4kb缓冲区大小
                const int chunkSize = 1024 * 4;
                var bytes = ArrayPool<byte>.Shared.Rent(chunkSize);
                var buffer = new Memory<byte>(bytes);

                try
                {
                    var result = await ClientWebSocket.ReceiveAsync(buffer, _cts.Token).ConfigureAwait(false);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        State = HubConnectionState.Disconnected;
                        await ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "<UNK>", CancellationToken.None);
                    }
                    else
                    {
                        ResponseMessage message = null;
                        if (result.EndOfMessage)
                        {
                            if (result.Count > 0)
                                message = ResponseMessage.BinaryMessage(buffer[..result.Count]);
                        }
                        else
                        {
                            await using var ms = MemoryStreamPool.GetStream();
                            await ms.WriteAsync(buffer[..result.Count], _cts.Token);
                            do
                            {
                                result = await ClientWebSocket.ReceiveAsync(buffer, _cts.Token).ConfigureAwait(false);
                                if (result.Count > 0) await ms.WriteAsync(buffer[..result.Count], _cts.Token);
                            } while (!result.EndOfMessage);
                            ms.Seek(0, SeekOrigin.Begin);
                            
                            message = ResponseMessage.BinaryStreamMessage(ms);
                        }
                        
                        if (message != null)
                            await MessageReceived.Invoke(message, _cts.Token);
                    }
                }
                catch (Exception ex)
                {
                    CheckReceiveMessageException(ex);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(bytes);
                }
            }

            await StopAsync(cancellationToken: _cts.Token);
            
        }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
    }

    /// <summary>
    ///     检查接收数据流异常
    /// </summary>
    /// <param name="ex"></param>
    private void CheckReceiveMessageException(Exception ex)
    {
        State = ClientWebSocket.State == WebSocketState.Open ? HubConnectionState.Connected : HubConnectionState.Disconnected;
                    
        if (State == HubConnectionState.Disconnected) Closed?.Invoke(ex);
    }
    #endregion
    
    #region Inner

    /// <summary>
    ///     远端通信连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task ConnectInnerAsync(CancellationToken cancellationToken = default)
    {
        await ClientWebSocket.ConnectAsync(_uri, cancellationToken);
    }

    #endregion
    
    #region Reconnect

    private readonly AtomicInteger _automaticReconnectCounter = new();

    /// <summary>
    ///     重连
    /// </summary>
    private void HandleReconnect()
    {
        if (_automaticReconnectCounter.IncrementAndGet() != 1) return;

        _ = Task.Factory.StartNew(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                if (ClientWebSocket != null && ClientWebSocket.State != WebSocketState.None && ClientWebSocket.State != WebSocketState.Open )
                {
                    State = HubConnectionState.Reconnecting;
                    Reconnecting?.Invoke();

                    try
                    {
                        await StartAsync(_cts.Token);
                        State = HubConnectionState.Connected;
                        Reconnected?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        if (ServiceLocator.IsProviderEnabled)
                        {
                            ServiceLocator.GetService<ILogger<HubConnection>>()?.LogError(ex, $"WebSocket通信“{_uri}”重连异常");
                        }
                        else
                        {
                            Console.WriteLine($"WebSocket通信“{_uri}”重连异常:{ex.Message}");
                        }
                    }
                }
                
                await Task.Delay(_reconnectInterval, _cts.Token);
            }
            // ReSharper disable once FunctionNeverReturns
        }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
    }

    #endregion
    
    #region Dispose

    private readonly AtomicInteger _disposeCounter = new();
    
    /// <summary>
    ///     释放资源
    /// </summary>
    /// <returns></returns>
    public virtual async ValueTask DisposeAsync()
    {
        if (_disposeCounter.IncrementAndGet() != 1) return;
        
        await StopAsync().ConfigureAwait(false);

        #if !NET8_0_OR_GREATER
            _cts.Cancel();
        #else
            await _cts.CancelAsync(); 
        #endif
        
        ClientWebSocket?.Abort();
        ClientWebSocket?.Dispose();
        ClientWebSocket = null;
    }

    #endregion
}
