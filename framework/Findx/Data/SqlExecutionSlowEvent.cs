using Findx.Messaging;

namespace Findx.Data;

/// <summary>
///     慢Sql事件
/// </summary>
public class SqlExecutionSlowEvent : IApplicationEvent
{
    /// <summary>
    ///     原始Sql
    /// </summary>
    public string SqlRaw { set; get; }

    /// <summary>
    ///     耗时毫秒数
    /// </summary>
    public long ElapsedMilliseconds { set; get; }
}