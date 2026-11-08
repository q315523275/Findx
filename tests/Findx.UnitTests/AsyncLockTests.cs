using Findx.Locks;

namespace Findx.UnitTests;

/// <summary>
///     AsyncLock 超时语义回归测试（修复：WaitAsync 超时返回 false 必须抛 TimeoutException，防止伪锁）
/// </summary>
public class AsyncLockTests
{
    [Fact]
    public async Task LockAsync_WhenContended_TimeoutThrows()
    {
        using var alock = new AsyncLock();
        using var held = await alock.LockAsync();

        // 锁被占用且超时极短：必须抛 TimeoutException，而不是返回"伪锁"
        await Assert.ThrowsAsync<TimeoutException>(() => alock.LockAsync(TimeSpan.FromMilliseconds(50), default));
    }

    [Fact]
    public async Task LockAsync_AfterRelease_CanAcquire()
    {
        using var alock = new AsyncLock();

        using (await alock.LockAsync())
        {
            // 占用中
        }

        // 释放后可再次获取
        using var second = await alock.LockAsync(TimeSpan.FromSeconds(1), default);
        Assert.NotNull(second);
    }

    [Fact]
    public async Task LockAsync_Contended_ReleasesAndOtherProceeds()
    {
        using var alock = new AsyncLock();
        // 手动管理释放：由后台任务在延迟后释放，避免 using 作用域双重释放
        var held = await alock.LockAsync();

        var waiting = alock.LockAsync(TimeSpan.FromSeconds(2), default);

        // 稍后释放，等待者应能获得锁
        _ = Task.Run(async () =>
        {
            await Task.Delay(100);
            held.Dispose();
        });

        using var acquired = await waiting;
        Assert.NotNull(acquired);
    }
}
