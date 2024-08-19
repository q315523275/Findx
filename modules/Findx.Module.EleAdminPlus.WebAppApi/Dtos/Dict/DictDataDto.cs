using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.Dict;

/// <summary>
///     字典项目数据
/// </summary>
public class DictDataDto: IResponse
{
    /// <summary>
    ///     字典项id
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
    
    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }
}