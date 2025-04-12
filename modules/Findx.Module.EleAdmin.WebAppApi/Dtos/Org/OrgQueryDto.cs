using Findx.Data;

namespace Findx.Module.EleAdmin.Dtos.Org;

/// <summary>
///     查询组织入参
/// </summary>
public class OrgQueryDto : PageBase
{
    /// <summary>
    ///     父级组织Id
    /// </summary>
    public Guid? Pid { set; get; }

    /// <summary>
    ///     关键词
    /// </summary>
    public string Keywords { set; get; }
    
    /// <summary>
    ///     类型
    /// </summary>
    public string Type { set; get; }
}