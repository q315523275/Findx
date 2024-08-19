using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdminPlus.Shared.Models;

/// <summary>
///     角色机构
/// </summary>
[Table(Name = "sys_role_org")]
[EntityExtension(DataSource = "system")]
public class SysRoleOrgInfo : EntityBase<long>
{
    /// <summary>
    ///     主键id
    /// </summary>
    [Column(IsPrimary = true, IsIdentity = false)]
    public override long Id { get; set; }

    /// <summary>
    ///     角色id
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    ///     机构id
    /// </summary>
    public long OrgId { get; set; }
    
    /// <summary>
    ///     机构信息
    /// </summary>
    [Navigate(nameof(OrgId))]
    public virtual SysOrgInfo OrgInfo { set; get; }
}