using System.Diagnostics;
using Findx.Locks;

namespace Findx.UnitTests;

/// <summary>
///     LockBase 等锁上限回归测试（修复：循环退出条件恒真 → startTime.Add(ttl) 比较，等满最大时长直接返回）
/// </summary>
public class LockBaseTests
{
    /// <summary>永远获取不到锁的派生实现：模拟资源一直被占用</summary>
    private sealed class AlwaysFailLock : LockBase
    {
        protected override Task<bool> TryLockAsync(string resource, string lockId, TimeSpan timeUntilExpires, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public override Task ReleaseAsync(string resource, string lockId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public override Task RenewAsync(string resource, string lockId, TimeSpan timeUntilExpires, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    [Fact]
    public async Task AcquireAsync_WaitNeverGetsLock_ReturnsNullWithinBoundedTime()
    {
        var lockImpl = new AlwaysFailLock();
        var sw = Stopwatch.StartNew();

        // 修复前：退出条件恒真 + 无取消 → 无限自旋；修复后：等满 timeUntilExpires 即返回 null
        var rlock = await lockImpl.AcquireAsync("resource", TimeSpan.FromMilliseconds(200), isWait: true);
        sw.Stop();

        Assert.Null(rlock);
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(3), $"应在有限时间内返回，实际耗时 {sw.Elapsed}");
    }

    [Fact]
    public async Task AcquireAsync_NoWait_ReturnsNullImmediately()
    {
        var lockImpl = new AlwaysFailLock();

        // isWait=false：一次尝试失败直接返回，不进入等待循环
        var rlock = await lockImpl.AcquireAsync("resource", TimeSpan.FromSeconds(30), isWait: false);

        Assert.Null(rlock);
    }
}
