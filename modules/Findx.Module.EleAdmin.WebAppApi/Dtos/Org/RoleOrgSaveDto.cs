using Findx.Data;
using Findx.Module.EleAdmin.Shared.Enum;

namespace Findx.Module.EleAdmin.Dtos.Org;

/// <summary>
///     设置角色数据范围
/// </summary>
public class RoleOrgSaveDto: IRequest
{
    /// <summary>
    ///     编号
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    ///     数据范围（1：全部数据权限 2：自定数据权限 3：本部门数据权限 4：本部门及以下数据权限）
    /// </summary>
    public DataScope DataScope { get; set; }

    /// <summary>
    ///     机构集合
    /// </summary>
    public List<Guid> OrgIds { get; set; } = [];
}