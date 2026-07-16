using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Vos.Dictionary;

/// <summary>
///     字典类型Vo
/// </summary>
public class DictionarySimplifyVo: IResponse
{
    /// <summary>
    ///     字典id
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
        
    /// <summary>
    ///     创建人
    /// </summary>
    public string Creator { get; set; }
    
    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }
    
    /// <summary>
    ///     最后更新人
    /// </summary>
    public string LastUpdater { get; set; }

    /// <summary>
    ///     最后更新时间
    /// </summary>
    public DateTime? LastUpdatedTime { get; set; }
}