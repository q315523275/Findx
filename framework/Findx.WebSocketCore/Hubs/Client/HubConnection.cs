using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Locks;
using Findx.Serialization;
using Findx.WebSocketCore.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace Findx.WebSocketCore.Hubs.Client;

/// <summary>
///     通信连接类
/// </summary>
public class HubConnection: IAsyncDisposable
{
    private readonly AsyncLock _lock = new();

    private readonly CancellationTokenSource _cts;
    
    private readonly TimeSpan _reconnectInterval = TimeSpan.FromSeconds(5);

    private readonly RecyclableMemoryStreamManager _memoryStreamPool = new();
    
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="url"></param>
    /// <param name="automaticReconnect">释放自动重连</param>
    public HubConnection(string url, bool automaticReconnect)
    {
        WebSocket = new ClientWebSocket();
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
    protected ClientWebSocket WebSocket;
    
    /// <summary>
    ///     开始连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    public virtual async Task StartAsync(CancellationToken cancellationToken = default)
    {
        using var asyncLock = await _lock.LockAsync(cancellationToken);
        
        if (WebSocket is { State: WebSocketState.Open })
        {
            State = HubConnectionState.Connected;
            return;
        }
        
        State = HubConnectionState.Connecting;

        await ConnectInnerAsync(cancellationToken);

        State = WebSocket.State == WebSocketState.Open ? HubConnectionState.Connected : HubConnectionState.Disconnected;
    }
    
    /// <summary>
    ///     关闭连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    public virtual async Task StopAsync(CancellationToken cancellationToken = default)
    {
        using var asyncLock = await _lock.LockAsync(cancellationToken);
        
        if (WebSocket is { State: WebSocketState.Open })
        {
            await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "主动停止...", cancellationToken);

            State = HubConnectionState.Disconnected;
            
            if (Closed != null) await Closed(null);
        }
    }
    
    /// <summary>
    ///     发送消息
    /// </summary>
    /// <param name="data"></param>
    /// <param name="messageType"></param>
    /// <param name="endOfMessage"></param>
    /// <param name="cancellationToken"></param>
    public virtual async Task SendAsync(ArraySegment<byte> data, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken = default)
    {
        await WebSocket.SendAsync(data, messageType, endOfMessage, cancellationToken);
    }
    
    /// <summary>
    ///     发送消息
    /// </summary>
    /// <param name="data"></param>
    /// <param name="messageType"></param>
    /// <param name="endOfMessage"></param>
    /// <param name="cancellationToken"></param>
    public virtual async Task SendAsync(ReadOnlyMemory<byte> data, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken = default)
    {
        await WebSocket.SendAsync(data, messageType, endOfMessage, cancellationToken);
    }

    /// <summary>
    ///     消息接收委托方法
    /// </summary>
    private Func<HubMessage, CancellationToken, Task> MessageReceived { set; get; }
    
    /// <summary>
    ///     消息事件
    /// </summary>
    /// <param name="received"></param>
    public void On<TMessage>(Func<TMessage, CancellationToken, Task> received) where TMessage: HubMessage
    {
        //MessageReceived += received;
        StartReceiveMessage();
    }

    #region Listen

    private readonly AtomicInteger _receivedCounter = new();
    
    /// <summary>
    ///     接收消息
    /// </summary>
    private void StartReceiveMessage()
    {
        if (_receivedCounter.IncrementAndGet() != 1) return;
        
        _ = Task.Factory.StartNew(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                if (WebSocket.State != WebSocketState.Open)
                {
                    await Task.Delay(_reconnectInterval);
                    continue;
                }

                var buffer = ArrayPool<byte>.Shared.Rent(8);
                var bufferSegment = new ArraySegment<byte>(buffer);

                try
                {
                    var result = await WebSocket.ReceiveAsync(bufferSegment, _cts.Token).ConfigureAwait(false);
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        State = HubConnectionState.Disconnected;
                        Closed?.Invoke(new Exception($"{result.CloseStatus}_{result.CloseStatusDescription}"));
                    }
                    else
                    {
                        
                        if (result.EndOfMessage)
                        {
                            if (bufferSegment.Array != null)
                            {
                                await HandleMessageAsync(result, bufferSegment.Array[Range.EndAt(result.Count)], _cts.Token);
                            }
                        }
                        else
                        {
                            // 创建一个 MemoryStream 来收集所有分段的消息数据
                            await using var ms = _memoryStreamPool.GetStream();

                            // 将第一个片段写入 MemoryStream
                            await ms.WriteAsync(bufferSegment.Array.AsMemory(bufferSegment.Offset, result.Count), _cts.Token);

                            do
                            {
                                result = await WebSocket.ReceiveAsync(bufferSegment, _cts.Token).ConfigureAwait(false);

                                if (bufferSegment.Array != null)
                                {
                                    // 将后续片段写入 MemoryStream
                                    await ms.WriteAsync(bufferSegment.Array.AsMemory(bufferSegment.Offset, result.Count), _cts.Token);
                                }
                            } while (!result.EndOfMessage);

                            // 重置 MemoryStream 位置
                            ms.Seek(0, SeekOrigin.Begin);

                            await HandleMessageAsync(result, ms, _cts.Token);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CheckReceiveMessageException(ex);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
    }

    /// <summary>
    ///     检查接收数据流异常
    /// </summary>
    /// <param name="ex"></param>
    private void CheckReceiveMessageException(Exception ex)
    {
        State = WebSocket.State == WebSocketState.Open ? HubConnectionState.Connected : HubConnectionState.Disconnected;
                    
        if (State == HubConnectionState.Disconnected) Closed?.Invoke(ex);
    }
    
    /// <summary>
    ///     处理消息
    /// </summary>
    /// <param name="result"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    private async Task HandleMessageAsync(WebSocketReceiveResult result, byte[] data, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(data.ToString2());
        // var rs = JsonSerializer.Deserialize<HubMessage>(data, SystemTextUtf8ByteSerializer.Options);
        // switch (result.MessageType)
        // {
        //     case WebSocketMessageType.Text when MessageReceived != null:
        //         await MessageReceived.Invoke(rs, cancellationToken).ConfigureAwait(false);
        //         break;
        //     case WebSocketMessageType.Close:
        //         State = HubConnectionState.Disconnected;
        //         Closed?.Invoke(new Exception($"{result.CloseStatus}_{result.CloseStatusDescription}"));
        //         break;
        //     case WebSocketMessageType.Binary:
        //         break;
        //     default:
        //         throw new ArgumentOutOfRangeException();
        // }
        await Task.CompletedTask;
    }
    
    /// <summary>
    ///     处理消息
    /// </summary>
    /// <param name="result"></param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    private async Task HandleMessageAsync(WebSocketReceiveResult result, Stream stream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        Console.WriteLine(await reader.ReadToEndAsync().ConfigureAwait(false));
        // var rs = await JsonSerializer.DeserializeAsync<HubMessage>(stream, options: SystemTextUtf8ByteSerializer.Options, cancellationToken: cancellationToken);
        // switch (result.MessageType)
        // {
        //     case WebSocketMessageType.Binary when MessageReceived != null:
        //     case WebSocketMessageType.Text when MessageReceived != null:
        //         await MessageReceived.Invoke(rs, cancellationToken).ConfigureAwait(false);
        //         break;
        //     case WebSocketMessageType.Close:
        //         State = HubConnectionState.Disconnected;
        //         Closed?.Invoke(new Exception($"{result.CloseStatus}_{result.CloseStatusDescription}"));
        //         break;
        //     default:
        //         throw new ArgumentOutOfRangeException();
        // }
    }
    #endregion
    
    #region Inner

    /// <summary>
    ///     远端通信连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task ConnectInnerAsync(CancellationToken cancellationToken = default)
    {
        await WebSocket.ConnectAsync(_uri, cancellationToken);
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
        
        var logger = ServiceLocator.GetService<ILogger<HubConnection>>();
        
        _ = Task.Factory.StartNew(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                if (WebSocket is { State: WebSocketState.Closed })
                {
                    State = HubConnectionState.Reconnecting;
                    Reconnecting?.Invoke();

                    try
                    {
                        await ConnectInnerAsync(_cts.Token);
                        State = HubConnectionState.Connected;
                        Reconnected?.Invoke();
                    }
                    catch (Exception ex)
                    {
                       logger?.LogError(ex, "WebSocket通信“{S}”重连异常", _uri.ToString());
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
        
        WebSocket.Abort();
        WebSocket.Dispose();
        WebSocket = null;
    }

    #endregion
}
