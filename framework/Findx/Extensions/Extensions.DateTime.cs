using System.Globalization;

namespace Findx.Extensions;

/// <summary>
///     系统扩展 - 时间
/// </summary>
public static partial class Extensions
{
    /// <summary>
    ///     是否周末
    /// </summary>
    /// <param name="dayOfWeek"></param>
    /// <returns></returns>
    public static bool IsWeekend(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek.IsIn(DayOfWeek.Saturday, DayOfWeek.Sunday);
    }

    /// <summary>
    ///     将时间转换为时间戳
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="milliseconds">是否分钟级别</param>
    /// <returns></returns>
    public static string ToJsGetTime(this DateTime dateTime, bool milliseconds = true)
    {
        var timeSpan = dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1));
        return Math.Round(milliseconds ? timeSpan.TotalMilliseconds : timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     将JS时间格式的数值转换为时间
    /// </summary>
    public static DateTime FromJsGetTime(this long jsTime)
    {
        var length = jsTime.ToString().Length;
        if (length != 10 && length != 13)
            throw new Exception("JS时间数值的长度不正确，必须为10位或13位");
        var start = new DateTime(1970, 1, 1);
        var result = length == 10 ? start.AddSeconds(jsTime) : start.AddMilliseconds(jsTime);
        return result.ToUniversalTime();
    }
    
    /// <summary>
    ///     获取指定日期所在月份的第一天
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static DateTime GetFirstDayOfMonth(this DateTime value)
    {
        var dtFrom = value;
        dtFrom = dtFrom.AddDays(-(dtFrom.Day - 1));
        return dtFrom;
    }
    
    /// <summary>
    ///     获取指定日期所在月份的最后一天
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static DateTime GetLastDayOfMonth(this DateTime value)
    {
        var dtTo = value;
        dtTo = dtTo.AddMonths(1);
        dtTo = dtTo.AddDays(-(dtTo.Day));
        return dtTo;
    }
    
    /// <summary>
    /// Converts a DateTime to ISO 8601 string
    /// </summary>
    public static string ToIso8601String(this DateTime value)
    {
        return value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }
}