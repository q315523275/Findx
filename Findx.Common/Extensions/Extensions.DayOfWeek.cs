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
    }
}
