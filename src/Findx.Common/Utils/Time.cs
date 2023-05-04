namespace Findx.Utils;

/// <summary>
///     时间转换工具类
/// </summary>
public static class Time
{
    /// <summary>
    ///     字符串时间值转TimsSpan
    /// </summary>
    /// <param name="time">如:10s</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(string time)
    {
        if (string.IsNullOrWhiteSpace(time)) throw new ArgumentNullException(nameof(time));

        time = time.ToLower();

        if (time.EndsWith("ms")) return ToTimeSpan(int.Parse(time.Substring(0, time.Length - 2)), "ms");

        if (time.EndsWith("s")) return ToTimeSpan(int.Parse(time.Substring(0, time.Length - 1)), "s");

        if (time.EndsWith("m")) return ToTimeSpan(int.Parse(time.Substring(0, time.Length - 1)), "m");

        if (time.EndsWith("h")) return ToTimeSpan(int.Parse(time.Substring(0, time.Length - 1)), "h");

        throw new InvalidOperationException("Incorrect format:" + time);
    }

    /// <summary>
    ///     值转换为TimeSpan
    /// </summary>
    /// <param name="value"></param>
    /// <param name="unit">值单位,如:ms,s,h</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(int value, string unit)
    {
        if (string.IsNullOrWhiteSpace(unit)) throw new ArgumentNullException(nameof(unit));

        switch (unit)
        {
            case "ms":
                return TimeSpan.FromMilliseconds(value);

            case "s":
                return TimeSpan.FromSeconds(value);

            case "m":
                return TimeSpan.FromMinutes(value);

            case "h":
                return TimeSpan.FromHours(value);
        }

        throw new InvalidOperationException("Incorrect unit:" + unit);
    }
}