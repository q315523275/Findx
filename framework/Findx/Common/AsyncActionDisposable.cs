using System.Threading.Tasks;

namespace Findx.Common;

/// <summary>
///     允许在处置时执行操作
/// </summary>
public struct AsyncActionDisposable : IAsyncDisposable
{
    readonly Func<ValueTask> _action;

    /// <summary>
    ///     Default Empty
    /// </summary>
    public static readonly AsyncActionDisposable Empty = new(() => new ValueTask(Task.CompletedTask));

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="action"></param>
    public AsyncActionDisposable(Func<ValueTask> action)
    {
        _action = action;
    }

    /// <summary>
    ///     DisposeAsync
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _action().ConfigureAwait(false);
    }
}