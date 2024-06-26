#nullable enable
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;

/// <summary>
///     进程异步迭代器
/// </summary>
// ReSharper disable once CheckNamespace
internal class ProcessAsyncEnumerator : IAsyncEnumerator<string>
{
    private readonly Process? _process;
    private readonly ChannelReader<string> _channel;
    private readonly CancellationToken _cancellationToken;
    private readonly CancellationTokenRegistration _cancellationTokenRegistration;
    private string? _current;
    private bool _disposed;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="process"></param>
    /// <param name="channel"></param>
    /// <param name="cancellationToken"></param>
    public ProcessAsyncEnumerator(Process? process, ChannelReader<string> channel, CancellationToken cancellationToken)
    {
        // process is not null, kill when canceled.
        _process = process;
        _channel = channel;
        _cancellationToken = cancellationToken;
        if (cancellationToken.CanBeCanceled)
        {
            _cancellationTokenRegistration = cancellationToken.Register(() => { _ = DisposeAsync(); });
        }
    }

#pragma warning disable CS8603
    // when call after MoveNext, current always not null.
    public string Current => _current;
#pragma warning restore CS8603

    public async ValueTask<bool> MoveNextAsync()
    {
        if (_channel.TryRead(out _current))
        {
            return true;
        }

        if (await _channel.WaitToReadAsync(_cancellationToken).ConfigureAwait(false))
        {
            if (_channel.TryRead(out _current))
            {
                return true;
            }
        }

        return false;
    }

    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            try
            {
                _cancellationTokenRegistration.Dispose();
                if (_process != null)
                {
                    _process.EnableRaisingEvents = false;
                    if (!_process.HasExited)
                    {
                        _process.Kill();
                    }
                }
            }
            finally
            {
                if (_process != null)
                {
                    _process.Dispose();
                }
            }
        }

        return default;
    }
}