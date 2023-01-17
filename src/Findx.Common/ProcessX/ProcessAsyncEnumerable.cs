#nullable enable
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;

/// <summary>
/// 进程异步可迭代集合
/// </summary>
public class ProcessAsyncEnumerable : IAsyncEnumerable<string>
{
    readonly Process? _process;
    readonly ChannelReader<string> _channel;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="process"></param>
    /// <param name="channel"></param>
    internal ProcessAsyncEnumerable(Process? process, ChannelReader<string> channel)
    {
        _process = process;
        _channel = channel;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new ProcessAsyncEnumerator(_process, _channel, cancellationToken);
    }

    /// <summary>
    /// Consume all result and wait complete asynchronously.
    /// </summary>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        await foreach (var _ in this.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
        }
    }

    /// <summary>
    /// Returning first value and wait complete asynchronously.
    /// </summary>
    public async Task<string> FirstAsync(CancellationToken cancellationToken = default)
    {
        string? data = null;
        await foreach (var item in this.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            data ??= (item ?? "");
        }

        if (data == null)
        {
            throw new InvalidOperationException("Process does not return any data.");
        }

        return data;
    }

    /// <summary>
    /// Returning first value or null and wait complete asynchronously.
    /// </summary>
    public async Task<string?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        string? data = null;
        await foreach (var item in this.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            data ??= (item ?? "");
        }

        return data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<string>> ToListAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<string>();
        await foreach (var item in this.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            list.Add(item);
        }

        return list;
    }

    /// <summary>
    /// Write the all received data to console.
    /// </summary>
    public async Task WriteLineAllAsync(CancellationToken cancellationToken = default)
    {
        await foreach (var item in this.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            Console.WriteLine(item);
        }
    }
}