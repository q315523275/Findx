using Findx.Extensions.Scheduler.Scheduling;

namespace Findx.UnitTests;

/// <summary>
///     重试策略纯函数测试（RetryAtTime 语义）：死信阈值判定 + 退避重试时刻计算（只写 RetryAtTime，周期锚点不偏移）
/// </summary>
public class SchedulerRetryPolicyTests
{
    private static readonly DateTime FixedNow = new(2026, 11, 3, 12, 0, 0, DateTimeKind.Local);

    // ---------- 死信阈值判定 ----------

    [Fact]
    public void IsRetryExhausted_SixthAttemptWithMaxFive_ReturnsTrue()
    {
        // 第 6 次尝试 = 已重试 5 次仍失败 → 进死信
        Assert.True(RetryPolicy.IsRetryExhausted(6, 5));
    }

    [Fact]
    public void IsRetryExhausted_FifthAttemptWithMaxFive_ReturnsFalse()
    {
        // 第 5 次尝试仍在重试预算内
        Assert.False(RetryPolicy.IsRetryExhausted(5, 5));
    }

    [Fact]
    public void IsRetryExhausted_FirstAttemptWithMaxZero_ReturnsTrue()
    {
        // MaxRetryCount=0：任何失败即进死信
        Assert.True(RetryPolicy.IsRetryExhausted(1, 0));
    }

    [Fact]
    public void IsRetryExhausted_NegativeAttempt_NormalizedToZero()
    {
        Assert.False(RetryPolicy.IsRetryExhausted(-1, 5));
    }

    // ---------- 退避重试时刻（RetryAtTime 语义） ----------

    [Fact]
    public void CalculateRetryAt_FirstRetry_BaseDelayLater()
    {
        var retryAt = RetryPolicy.CalculateRetryAt(FixedNow, 1, 1000, 60000);
        Assert.Equal(FixedNow.AddSeconds(1), retryAt);
    }

    [Fact]
    public void CalculateRetryAt_SecondRetry_DoubleBaseDelay()
    {
        var retryAt = RetryPolicy.CalculateRetryAt(FixedNow, 2, 1000, 60000);
        Assert.Equal(FixedNow.AddSeconds(2), retryAt);
    }

    [Fact]
    public void CalculateRetryAt_SeventhRetry_CappedAtMaxDelay()
    {
        // 第 7 次重试 2^6=64000ms > 60000ms → 封顶
        var retryAt = RetryPolicy.CalculateRetryAt(FixedNow, 7, 1000, 60000);
        Assert.Equal(FixedNow.AddSeconds(60), retryAt);
    }

    [Fact]
    public void CalculateRetryAt_MaxLessThanBase_FallsBackToBase()
    {
        var retryAt = RetryPolicy.CalculateRetryAt(FixedNow, 1, 1000, 500);
        Assert.Equal(FixedNow.AddSeconds(1), retryAt);
    }

    [Fact]
    public void CalculateRetryAt_ZeroRetryNumber_NormalizedToFirstRetry()
    {
        var retryAt = RetryPolicy.CalculateRetryAt(FixedNow, 0, 1000, 60000);
        Assert.Equal(FixedNow.AddSeconds(1), retryAt);
    }

    [Fact]
    public void CalculateRetryAt_RetryTimeIsAlwaysInFuture_DoesNotTouchPeriodAnchor()
    {
        // 重试时刻永远在未来（退避 ≥ base）；且该函数只产出 RetryAtTime，不涉及 NextRunTime——周期锚点由 CAS 单独推进
        var retryAt = RetryPolicy.CalculateRetryAt(FixedNow, 3, 2000, 60000);
        Assert.True(retryAt > FixedNow);
        Assert.Equal(FixedNow.AddSeconds(8), retryAt);
    }

    [Fact]
    public void IsRetryExhausted_CombinedWithRetryAt_LastRetryThenDeadLetter()
    {
        // 第 5 次尝试允许（IsRetryExhausted=false），其退避后若再失败 → 第 6 次尝试进死信
        var lastRetryAt = RetryPolicy.CalculateRetryAt(FixedNow, 5, 1000, 60000);
        Assert.Equal(FixedNow.AddSeconds(16), lastRetryAt);
        Assert.False(RetryPolicy.IsRetryExhausted(5, 5));
        Assert.True(RetryPolicy.IsRetryExhausted(6, 5));
    }
}
