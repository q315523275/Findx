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
    /// <param name="milsec">是否分钟级别</param>
    /// <returns></returns>
    public static string ToJsGetTime(this DateTime dateTime, bool milsec = true)
    {
        var timeSpan = dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1));
        return Math.Round(milsec ? timeSpan.TotalMilliseconds : timeSpan.TotalSeconds).ToString();
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
}