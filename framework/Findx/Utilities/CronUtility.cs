using Findx.Common;
using Findx.Cronos;

namespace Findx.Utilities;

/// <summary>
///     Cron操作类
/// </summary>
public static class CronUtility
{
    /// <summary>
    ///     根据Cron表达式获取下次执行时间
    /// </summary>
    /// <param name="cronExpress"></param>
    /// <returns></returns>
    public static DateTime GetNextOccurrence(string cronExpress)
    {
        return GetNextOccurrence(cronExpress, DateTimeOffset.UtcNow);
    }

    /// <summary>
    ///     根据Cron表达式获取下次执行时间
    /// </summary>
    /// <param name="cronExpress"></param>
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    public static DateTime GetNextOccurrence(string cronExpress, DateTimeOffset dateTimeOffset)
    {
        Check.NotNull(cronExpress, nameof(cronExpress));

        var items = cronExpress.Split(" ");

        CronExpression expression;
        if (items.Length == 6)
            expression = CronExpression.Parse(cronExpress, CronFormat.IncludeSeconds);
        else
            expression = CronExpression.Parse(cronExpress);

        var next = expression.GetNextOccurrence(dateTimeOffset, TimeZoneInfo.Local);

        return next.HasValue ? next.Value.LocalDateTime : default;
    }
}