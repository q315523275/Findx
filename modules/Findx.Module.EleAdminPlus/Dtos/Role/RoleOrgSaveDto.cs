using Findx.Data;

namespace Findx.Module.EleAdminPlus.Dtos.Role;

/// <summary>
///     设置角色数据范围
/// </summary>
public class RoleOrgSaveDto: IRequest
{
    /// <summary>
    ///     编号
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    ///     数据范围（1：全部数据权限 2：自定数据权限 3：本部门数据权限 4：本部门及以下数据权限）
    /// </summary>
    public int DataScope { get; set; } = 1;

    /// <summary>
    ///     机构集合
    /// </summary>
    public List<long> OrgIds { get; set; } = [];
}