namespace Findx.Common;

/// <summary>
/// 允许在处置时执行操作
/// </summary>
public struct ActionDisposable : IDisposable
{
    private readonly Action _action;

    /// <summary>
    /// Ctor
    /// </summary>
    public static readonly ActionDisposable Empty = new(() => { });

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="action"></param>
    public ActionDisposable(Action action)
    {
        Check.NotNull(action, nameof(action));

        _action = action;
    }

    /// <summary>
    /// 释放
    /// </summary>
    public void Dispose()
    {
        _action();
    }
}