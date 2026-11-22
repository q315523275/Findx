using Findx.Common;
using Findx.Messaging;

namespace Findx.Data;

/// <summary>
///     Sql慢查询事件
/// </summary>
public class SqlSlowQueryEvent : IApplicationEvent, IAsync
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