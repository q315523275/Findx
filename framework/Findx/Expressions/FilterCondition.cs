using System.Text.Json.Serialization;

namespace Findx.Expressions;

/// <summary>
///     筛选规则
/// </summary>
public class FilterCondition
{
    /// <summary>
    ///     字段名称
    /// </summary>
    public string Field { get; set; }
    
    /// <summary>
    ///     字段值
    /// </summary>
    public string Value { get; set; }
    
    /// <summary>
    ///     筛选方式
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FilterOperate Operator { get; set; }
}