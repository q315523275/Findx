using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Findx.Linq;

/// <summary>
///     排序规则
/// </summary>
public class OrderConditions
{
    /// <summary>
    ///     字段名称
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    ///     筛选方式
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ListSortDirection SortDirection { set; get; } = ListSortDirection.Descending;
}