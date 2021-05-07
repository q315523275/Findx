using System;

namespace Findx.Extensions
{
    /// <summary>
    /// 系统扩展 - 星期
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 是否周末
        /// </summary>
        /// <param name="dayOfWeek"></param>
        /// <returns></returns>
        public static bool IsWeekend(this DayOfWeek dayOfWeek)
        {
            return dayOfWeek.IsIn(DayOfWeek.Saturday, DayOfWeek.Sunday);
        }
        /// <summary>
        /// 将时间转换为时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="milsec">是否分钟级别</param>
        /// <returns></returns>
        public static string ToJsGetTime(this DateTime dateTime, bool milsec = true)
        {
            TimeSpan timeSpan = dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1));
            return Math.Round(milsec ? timeSpan.TotalMilliseconds : timeSpan.TotalSeconds).ToString();
        }
    }
}
