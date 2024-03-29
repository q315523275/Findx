﻿using System.Threading.Tasks;

namespace Findx.Extensions;

/// <summary>
///     扩展 - 时间片
/// </summary>
public static partial class Extensions
{
    /// <summary>
    ///     将时间片转换为X时X分X秒的格式
    /// </summary>
    /// <param name="ts"></param>
    /// <returns></returns>
    public static string ToTimeString(this TimeSpan ts)
    {
        var value = string.Empty;
        if (ts.Seconds > 0) value += $"{ts.Seconds:D2}秒";

        if (ts.Minutes > 0) value = $"{ts.Minutes}分{value}";

        if (ts.Hours > 0) value = $"{ts.Hours}时{value}";

        if (ts.Days > 0) value = $"{ts.Days}天{value}";

        return value;
    }

    /// <summary>
    ///     时间片倒计时
    /// </summary>
    public static void CountDown(this TimeSpan ts, Action<TimeSpan> action, int intervalMilliseconds = 1000)
    {
        while (ts > TimeSpan.Zero)
        {
            action(ts);
            var ts2 = TimeSpan.FromMilliseconds(intervalMilliseconds);
            Thread.Sleep(ts2);
            ts = ts.Subtract(ts2);
        }
    }

    /// <summary>
    ///     时间片倒计时
    /// </summary>
    public static async Task CountDownAsync(this TimeSpan ts, Func<TimeSpan, Task> action, int intervalMilliseconds = 1000)
    {
        while (ts > TimeSpan.Zero)
        {
            await action(ts);
            var ts2 = TimeSpan.FromMilliseconds(intervalMilliseconds);
            await Task.Delay(ts2);
            ts = ts.Subtract(ts2);
        }
    }
}