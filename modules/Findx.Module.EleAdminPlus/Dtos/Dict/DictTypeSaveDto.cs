using Findx.Data;

namespace Findx.Module.EleAdminPlus.Dtos.Dict;

/// <summary>
///     设置字典类型入参
/// </summary>
public class DictTypeSaveDto : IRequest<long>
{
    /// <summary>
    ///     编号
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     字典标识
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     字典名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     排序号
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     备注
    /// </summary>
    public string Comments { get; set; }
}