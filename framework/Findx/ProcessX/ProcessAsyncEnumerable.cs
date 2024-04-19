#nullable enable
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;

/// <summary>
///     进程异步可迭代集合
/// </summary>
// ReSharper disable once CheckNamespace
public class ProcessAsyncEnumerable : IAsyncEnumerable<string>
{
    private readonly Process? _process;
    private readonly ChannelReader<string> _channel;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="process"></param>
    /// <param name="channel"></param>
    internal ProcessAsyncEnumerable(Process? process, ChannelReader<string> channel)
    {
        _process = process;
        _channel = channel;
    }

    /// <summary>
    ///     返回异步流集合
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new ProcessAsyncEnumerator(_process, _channel, cancellationToken);
    }

    /// <summary>
    ///     使用所有结果并异步等待完成。
    /// </summary>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        await foreach (var _ in this.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
        }
    }

    /// <summary>
    ///     返回第一个值并异步等待完成。
    /// </summary>
    public async Task<string> FirstAsync(CancellationToken cancellationToken = default)
    {
        string? data = null;
        await foreach (var item in this.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (data == null)
            {
                data = (item ?? "");
            }
        }

        if (data == null)
        {
            throw new InvalidOperationException("Process does not return any data.");
        }

        return data;
    }

    /// <summary>
    ///     异步返回第一个值或null并等待完成。
    /// </summary>
    public async Task<string?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        string? data = null;
        await foreach (var item in this.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (data == null)
            {
                data = (item ?? "");
            }
        }

        return data;
    }

    /// <summary>
    ///     异步获取结果并转换为字符串集合
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string[]> ToTask(CancellationToken cancellationToken = default)
    {
        var list = new List<string>();
        await foreach (var item in this.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            list.Add(item);
        }

        return list.ToArray();
    }

    /// <summary>
    ///     Write the all received data to console.
    /// </summary>
    public async Task WriteLineAllAsync(CancellationToken cancellationToken = default)
    {
        await foreach (var item in this.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            Console.WriteLine(item);
        }
    }
}