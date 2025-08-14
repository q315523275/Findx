namespace Findx.Common.DisposableEx;

// 一般使用于方法返回,如using释放等等

/// <summary>
///     允许在释放时执行操作
/// </summary>
public readonly struct ActionDisposable : IDisposable
{
    private readonly Action _action;

    /// <summary>
    ///     Ctor
    /// </summary>
    public static readonly ActionDisposable Empty = new(() => { });

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="action"></param>
    public ActionDisposable(Action action)
    {
        Check.NotNull(action, nameof(action));

        _action = action;
    }

    /// <summary>
    ///     释放
    /// </summary>
    public void Dispose()
    {
        _action();
    }
}