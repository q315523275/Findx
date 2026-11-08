namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     重试退避延迟计算（纯函数，便于单测）：delay = min(base * 2^attempt, max)
/// </summary>
public static class RetryDelayCalculator
{
    /// <summary>
    ///     计算第 attempt 次重试前应等待的毫秒数（attempt 从 0 开始：0->base, 1->2*base, ...）
    /// </summary>
    public static int Calculate(int attempt, int baseDelayMs, int maxDelayMs)
    {
        if (attempt < 0) attempt = 0;
        if (baseDelayMs <= 0) baseDelayMs = 1000;
        if (maxDelayMs < baseDelayMs) maxDelayMs = baseDelayMs;

        // base * 2^attempt，防 long 溢出截断
        long delay;
        try
        {
            delay = checked((long)baseDelayMs * (1L << attempt));
        }
        catch (OverflowException)
        {
            return maxDelayMs;
        }

        return delay >= maxDelayMs ? maxDelayMs : (int)delay;
    }
}
