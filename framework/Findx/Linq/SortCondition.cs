using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Findx.Linq;

/// <summary>
///     列表字段排序条件
/// </summary>
public class SortCondition
{
    /// <summary>
    /// 获取或设置 排序字段名称
    /// </summary>
    public string SortField { get; set; }

    /// <summary>
    /// 获取或设置 排序方向
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;
}