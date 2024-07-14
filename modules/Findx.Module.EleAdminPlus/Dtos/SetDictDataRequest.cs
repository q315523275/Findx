using Findx.Data;

namespace Findx.Module.EleAdminPlus.Dtos;

/// <summary>
///     设置字典数据Dto模型
/// </summary>
public class SetDictDataRequest : IRequest<long>
{
    /// <summary>
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     字典id
    /// </summary>
    public long TypeId { get; set; }

    /// <summary>
    ///     字典项名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     字典项值
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///     排序号
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     备注
    /// </summary>
    public string Comments { get; set; }
}