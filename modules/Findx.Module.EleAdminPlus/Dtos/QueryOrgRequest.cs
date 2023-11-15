using Findx.Data;

namespace Findx.Module.EleAdminPlus.Dtos;

/// <summary>
///     查询组织入参
/// </summary>
public class QueryOrgRequest : PageBase
{
    /// <summary>
    ///     父级组织Id
    /// </summary>
    public long? Pid { set; get; }

    /// <summary>
    ///     关键词
    /// </summary>
    public string Keywords { set; get; }
}