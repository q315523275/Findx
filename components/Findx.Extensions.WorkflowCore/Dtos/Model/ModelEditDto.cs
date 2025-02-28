using Findx.Data;

namespace Findx.Extensions.WorkflowCore.Dtos.Model;

/// <summary>
///     模型编辑参数Dto
/// </summary>
public class ModelEditDto: ModelAddDto, IRequest<long>
{
    /// <summary>
    ///     记录Id
    /// </summary>
    public long Id { get; set; }
}