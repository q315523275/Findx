using System.Diagnostics;
using System.Threading.Tasks;

namespace Findx.Common;

/// <summary>
/// Disposable 基类
/// </summary>
public abstract class Disposable : IDisposable, IAsyncDisposable
{
    private const int DisposedFlag = 1;
    private int _isDisposed;

    /// <summary>
    /// 释放资源
    /// </summary>
    [DebuggerStepThrough]
    public void Dispose()
    {
        var wasDisposed = Interlocked.Exchange(ref _isDisposed, DisposedFlag);
        if (wasDisposed == DisposedFlag)
        {
            return;
        }

        OnDispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放非托管和（可选）托管的资源。
    /// </summary>
    /// <param name="disposing"><c>true</c>释放托管和非托管资源 <c>false</c> 只释放非托管资源.</param>
    protected virtual void OnDispose(bool disposing)
    {
    }

    /// <summary>
    /// 异步释放资源
    /// </summary>
    /// <returns></returns>
    [DebuggerStepThrough]
    public ValueTask DisposeAsync()
    {
        // 是否已释放
        var wasDisposed = Interlocked.Exchange(ref _isDisposed, DisposedFlag);
        if (wasDisposed != DisposedFlag)
        {
            GC.SuppressFinalize(this);
            return OnDisposeAsync(true);
        }

        return default;
    }

    /// <summary>
    ///  释放非托管和（可选）托管的资源。
    /// </summary>
    /// &lt;param name="disposing"&gt;&lt;c&gt;true&lt;/c&gt;释放托管和非托管资源 &lt;c&gt;false&lt;/c&gt; 只释放非托管资源.&lt;/param&gt;
    protected virtual ValueTask OnDisposeAsync(bool disposing)
    {
        // 默认实现执行同步处置。
        OnDispose(disposing);
        return default;
    }

    /// <summary>
    /// 当前实例是否已被释放。
    /// </summary>
    protected internal bool IsDisposed
    {
        get
        {
            Interlocked.MemoryBarrier();
            return _isDisposed == DisposedFlag;
        }
    }
    
    #region Utils for inheritors

    /// <summary>
    /// 检查资源是否已释放
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    [DebuggerStepThrough]
    protected internal void CheckDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    /// <summary>
    /// 检查资源是否已释放
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <exception cref="ObjectDisposedException"></exception>
    [DebuggerStepThrough]
    protected internal void CheckDisposed(string errorMessage)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(GetType().FullName, errorMessage);
    }

    /// <summary>
    /// 释放集合资源
    /// </summary>
    /// <param name="enumerable"></param>
    protected static void DisposeEnumerable(IEnumerable enumerable)
    {
        if (enumerable != null)
        {
            foreach (object obj2 in enumerable)
            {
                DisposeMember(obj2);
            }
            DisposeMember(enumerable);
        }
    }

    /// <summary>
    /// 释放字典资源
    /// </summary>
    /// <param name="dictionary"></param>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    protected static void DisposeDictionary<TK, TV>(IDictionary<TK, TV> dictionary)
    {
        if (dictionary != null)
        {
            foreach (var pair in dictionary)
            {
                DisposeMember(pair.Value);
            }
            DisposeMember(dictionary);
        }
    }

    /// <summary>
    /// 释放字典资源
    /// </summary>
    /// <param name="dictionary"></param>
    protected static void DisposeDictionary(IDictionary dictionary)
    {
        if (dictionary != null)
        {
            foreach (IDictionaryEnumerator pair in dictionary)
            {
                DisposeMember(pair.Value);
            }
            DisposeMember(dictionary);
        }
    }

    /// <summary>
    /// 释放成员资源
    /// </summary>
    /// <param name="member"></param>
    protected static void DisposeMember(object member)
    {
        if (member is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    #endregion
}