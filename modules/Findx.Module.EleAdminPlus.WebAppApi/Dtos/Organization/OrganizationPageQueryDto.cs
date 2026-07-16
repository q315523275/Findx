using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.Organization;

/// <summary>
///     分页查询组织入参
/// </summary>
public partial class OrganizationPageQueryDto: OrganizationQueryDto, IPager
{
    /// <summary>
    ///     页码
    /// </summary>
    public int PageNo { get; set; }
    
    /// <summary>
    ///     每页数量
    /// </summary>
    public int PageSize { get; set; }
}