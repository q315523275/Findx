using Findx.Extensions.Scheduler.Scheduling;

namespace Findx.UnitTests;

/// <summary>
///     重试退避计算纯函数测试：delay = min(base * 2^attempt, max)
/// </summary>
public class SchedulerRetryDelayTests
{
    [Fact]
    public void Calculate_AttemptZero_ReturnsBaseDelay()
    {
        Assert.Equal(1000, RetryDelayCalculator.Calculate(0, 1000, 60000));
    }

    [Fact]
    public void Calculate_AttemptOne_ReturnsDoubleBase()
    {
        Assert.Equal(2000, RetryDelayCalculator.Calculate(1, 1000, 60000));
    }

    [Fact]
    public void Calculate_AttemptTwo_ReturnsQuadrupleBase()
    {
        Assert.Equal(4000, RetryDelayCalculator.Calculate(2, 1000, 60000));
    }

    [Fact]
    public void Calculate_AttemptThree_ReturnsEightTimesBase()
    {
        Assert.Equal(8000, RetryDelayCalculator.Calculate(3, 1000, 60000));
    }

    [Fact]
    public void Calculate_ExceedsMax_ReturnsMaxCapped()
    {
        // 1000 * 2^6 = 64000 > 60000，封顶
        Assert.Equal(60000, RetryDelayCalculator.Calculate(6, 1000, 60000));
    }

    [Fact]
    public void Calculate_BaseNonPositive_FallsBackToDefaultBase()
    {
        Assert.Equal(1000, RetryDelayCalculator.Calculate(0, 0, 60000));
        Assert.Equal(1000, RetryDelayCalculator.Calculate(0, -500, 60000));
    }

    [Fact]
    public void Calculate_MaxSmallerThanBase_MaxAdjustedUpToBase()
    {
        // max < base 时按 base 封顶
        Assert.Equal(5000, RetryDelayCalculator.Calculate(0, 5000, 1000));
    }

    [Fact]
    public void Calculate_AttemptNegative_TreatedAsZero()
    {
        Assert.Equal(1000, RetryDelayCalculator.Calculate(-3, 1000, 60000));
    }

    [Fact]
    public void Calculate_AttemptHuge_NoOverflow_ThrowsNothing_ReturnsMax()
    {
        // 2^100 溢出 long，应走 catch 返回 max 而不抛异常
        var result = RetryDelayCalculator.Calculate(100, 1000, 60000);
        Assert.Equal(60000, result);
    }
}
