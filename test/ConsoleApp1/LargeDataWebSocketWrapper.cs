using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;

public class LargeDataWebSocketWrapper
{
    private readonly Uri _uri;
    private ClientWebSocket _clientWebSocket;
    private bool _isConnected = false;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private Pipe _readPipe;
    private Channel<ReadOnlyMemory<byte>> _messageChannel;

    public LargeDataWebSocketWrapper(Uri uri)
    {
        _uri = uri;
    }

    public async Task ConnectAsync()
    {
        if (_isConnected) return;
        
        _clientWebSocket = new ClientWebSocket();
        await _clientWebSocket.ConnectAsync(_uri, _cancellationTokenSource.Token);
        _isConnected = true;

        // 创建管道
        _readPipe = CreatePipe();

        // 创建 Channel
        _messageChannel = Channel.CreateUnbounded<ReadOnlyMemory<byte>>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = false
        });

        // 开始接收消息
        await ReceiveMessagesAsync();
    }

    private Pipe CreatePipe()
    {
        var pipeOptions = PipeOptions.Default;
        var pipe = new Pipe(pipeOptions);
        return pipe;
    }

    public async Task DisconnectAsync()
    {
        if (!_isConnected) return;
        
        _cancellationTokenSource.Cancel();
        await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        _clientWebSocket.Dispose();
        _clientWebSocket = null;
        _isConnected = false;

        // 关闭管道
        _readPipe.Writer.Complete();

        // 关闭 Channel
        _messageChannel.Writer.TryComplete();
    }

    public async Task SendMessageAsync(string message)
    {
        if (!_isConnected) return;
        
        var buffer = Encoding.UTF8.GetBytes(message);
        await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
    }

    private async Task ReceiveMessagesAsync()
    {
        var buffer = ArrayPool<byte>.Shared.Rent(1024 * 4); // 使用 ArrayPool

        try
        {
            while (_isConnected)
            {
                var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.Empty, result.CloseStatusDescription, CancellationToken.None);
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var receivedMessage = buffer.AsMemory(0, result.Count);

                    // 如果这是消息的开始部分
                    if (result.EndOfMessage)
                    {
                        // 整个消息都在这个分段中
                        await _messageChannel.Writer.WriteAsync(receivedMessage);
                    }
                    else
                    {
                        // 拼接分段消息
                        await HandlePartialMessageAsync(receivedMessage, result);
                    }
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async Task HandlePartialMessageAsync(ReadOnlyMemory<byte> currentPart, WebSocketReceiveResult result)
    {
        var messageBuilder = new SequenceCollector<byte>();

        // 添加当前分段
        messageBuilder.Add(currentPart);

        while (!_cancellationTokenSource.IsCancellationRequested && !result.EndOfMessage)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(1024 * 4);
            var nextResult = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);

            if (nextResult.MessageType == WebSocketMessageType.Close)
            {
                await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.Empty, nextResult.CloseStatusDescription, CancellationToken.None);
                break;
            }

            var nextPart = buffer.AsMemory(0, nextResult.Count);

            // 添加下一个分段
            messageBuilder.Add(nextPart);

            if (nextResult.EndOfMessage)
            {
                // 整个消息已经接收完毕
                var completeMessage = messageBuilder.ToReadOnlyMemory();
                await _messageChannel.Writer.WriteAsync(completeMessage);
                messageBuilder.Clear();
                break;
            }

            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public async Task ProcessBinaryAsync(Func<ReadOnlyMemory<byte>, ValueTask> processor)
    {
        await foreach (var message in _messageChannel.Reader.ReadAllAsync())
        {
            // 处理二进制数据
            await processor(message);
        }
    }
}

public struct SequenceCollector<T>
{
    private readonly List<ReadOnlyMemory<T>> _segments = [];
    private int _totalLength;

    public SequenceCollector()
    {
        _totalLength = 0;
    }

    public void Add(ReadOnlyMemory<T> memory)
    {
        _segments.Add(memory);
        _totalLength += memory.Length;
    }

    public ReadOnlyMemory<T> ToReadOnlyMemory()
    {
        if (_segments.Count == 0)
            return default;

        if (_segments.Count == 1)
            return _segments[0];

        // 如果有多个分段，需要将它们合并成一个大的内存区域
        using var memoryOwner = MemoryPool<T>.Shared.Rent(_totalLength);
        var destination = memoryOwner.Memory;

        var offset = 0;
        foreach (var segment in _segments)
        {
            segment.CopyTo(destination[offset..]);
            offset += segment.Length;
        }

        return destination;
    }

    public void Clear()
    {
        _segments.Clear();
        _totalLength = 0;
    }
}