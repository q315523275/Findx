using Findx.Data;

namespace Findx.Module.EleAdmin.Dtos;

/// <summary>
///     设置应用Dto模型
/// </summary>
public class SetAppRequest : IRequest<Guid>
{
    /// <summary>
    ///     编号
    /// </summary>
    public Guid Id { get; set; } = Guid.Empty;

    /// <summary>
    ///     编码
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     应用名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     状态（字典 0正常 1停用）
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    ///     排序
    /// </summary>
    public int Sort { get; set; }
}