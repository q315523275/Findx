using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdminPlus.Shared.Models;

/// <summary>
///     角色对应组织集合信息表
/// </summary>
[Table(Name = "sys_role_organizations")]
[EntityExtension(DataSource = "system")]
public class SysRoleOrgInfo : EntityBase<long>, ITenant<long>
{
    /// <summary>
    ///     主键id
    /// </summary>
    [Column(IsPrimary = true)]
    public override long Id { get; set; }

    /// <summary>
    ///     角色id
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    ///     组织id
    /// </summary>
    public long OrgId { get; set; }
    
    /// <summary>
    ///     租户id
    /// </summary>
    public long? TenantId { get; set; }
    
    /// <summary>
    ///     机构信息
    /// </summary>
    [Navigate(nameof(OrgId))]
    public virtual SysOrganizationInfo OrgInfo { set; get; }

}