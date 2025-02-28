using Findx.Data;

namespace Findx.Extensions.WorkflowCore.Dtos.Model;

/// <summary>
///     模型分页查询参数Dto
/// </summary>
public class ModelPageQueryDto: ModelQueryDto, IPager
{
    /// <summary>
    ///     页码
    /// </summary>
    public int PageNo { get; set; }
    
    /// <summary>
    ///     分页数量
    /// </summary>
    public int PageSize { get; set; }
}