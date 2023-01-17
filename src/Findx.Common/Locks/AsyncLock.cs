using System.Threading.Tasks;

namespace Findx.Locks;

/// <summary>
/// AsyncLock basedOn SemaphoreSlim
/// https://github.com/WeihanLi/WeihanLi.Common/blob/dev/src/WeihanLi.Common/AsyncLock.cs
/// </summary>
public sealed class AsyncLock: IDisposable
{
    private readonly SemaphoreSlim _mutex = new(1, 1);

    /// <summary>
    /// 同步锁
    /// </summary>
    /// <returns></returns>
    public IDisposable Lock()
    {
        _mutex.Wait();
        return new AsyncLockReleaser(_mutex);
    }

    /// <summary>
    /// 异步锁
    /// </summary>
    /// <returns></returns>
    public Task<IDisposable> LockAsync() => LockAsync(CancellationToken.None);

    /// <summary>
    /// 异步锁
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IDisposable> LockAsync(CancellationToken cancellationToken) => LockAsync(TimeSpan.Zero, cancellationToken);

    /// <summary>
    /// 异步锁
    /// </summary>
    /// <param name="timeout">超时</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IDisposable> LockAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (timeout <= TimeSpan.Zero)
        {
            await _mutex.WaitAsync(cancellationToken);
        }
        else
        {
            await _mutex.WaitAsync(timeout, cancellationToken);
        }
        return new AsyncLockReleaser(_mutex);
    }

    /// <summary>
    /// 释放
    /// </summary>
    public void Dispose()
    {
        _mutex.Dispose();
    }

    #region AsyncLockReleaser

    /// <summary>
    /// 锁释放器
    /// </summary>
    private struct AsyncLockReleaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphoreSlim;

        public AsyncLockReleaser(SemaphoreSlim semaphoreSlim) => _semaphoreSlim = semaphoreSlim;

        public void Dispose()
        {
            _semaphoreSlim.Release();
        }
    }

    #endregion AsyncLockReleaser
}