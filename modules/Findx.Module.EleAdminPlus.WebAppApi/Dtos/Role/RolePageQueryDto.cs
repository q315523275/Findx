using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.Role;

/// <summary>
///     分页查询角色入参
/// </summary>
public class RolePageQueryDto: RoleQueryDto, IPager
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