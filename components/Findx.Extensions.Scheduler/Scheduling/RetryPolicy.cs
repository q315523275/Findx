namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     重试策略纯函数（与 RetryDelayCalculator 同风格，便于单测）：
///     <para>1. 死信阈值判定：第 attempt 次尝试（1-based）是否已耗尽重试预算（attempt &gt; MaxRetryCount 进死信）；</para>
///     <para>2. 退避重试时刻：只产出 RetryAtTime，绝不改写周期锚点 NextRunTime——cron 节奏不因重试偏移。</para>
/// </summary>
public static class RetryPolicy
{
    /// <summary>
    ///     第 attempt 次尝试（1-based，首次尝试=1）是否已超过最大重试次数（达到即进死信）
    /// </summary>
    public static bool IsRetryExhausted(int attempt, int maxRetryCount)
    {
        if (attempt < 0) attempt = 0;
        return attempt > maxRetryCount;
    }

    /// <summary>
    ///     计算第 retryNumber 次重试（1-based，第 1 次重试=1）应发生的退避时刻：
    ///     delay = min(base * 2^(retryNumber-1), max)，仅写入 RetryAtTime，不影响 NextRunTime
    /// </summary>
    public static DateTime CalculateRetryAt(DateTime now, int retryNumber, int baseDelayMs, int maxDelayMs)
    {
        if (retryNumber < 1) retryNumber = 1;
        var delayMs = RetryDelayCalculator.Calculate(retryNumber - 1, baseDelayMs, maxDelayMs);
        return now.AddMilliseconds(delayMs);
    }
}
