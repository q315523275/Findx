using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Vos.DictData;

/// <summary>
///     字典项目数据Vo
/// </summary>
public class DictDataSimplifyVo: IResponse
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
    
    /// <summary>
    ///     字典简化信息
    /// </summary>
    public DictDataTypeSimplifyVo TypeInfo { get; set; }
}