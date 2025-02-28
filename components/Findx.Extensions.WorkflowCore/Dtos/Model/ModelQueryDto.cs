using Findx.Expressions;

namespace Findx.Extensions.WorkflowCore.Dtos.Model;

/// <summary>
///     模型查询参数Dto
/// </summary>
public class ModelQueryDto: SortCondition
{
    /// <summary>
    ///     模型名称
    /// </summary>
    public string Name { get; set; }
}