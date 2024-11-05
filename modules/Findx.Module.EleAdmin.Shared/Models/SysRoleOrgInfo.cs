using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdmin.Shared.Models;

/// <summary>
///     角色机构
/// </summary>
[Table(Name = "sys_role_org")]
[EntityExtension(DataSource = "system")]
public class SysRoleOrgInfo : EntityBase<Guid>
{
    /// <summary>
    ///     主键id
    /// </summary>
    [Column(IsPrimary = true, IsIdentity = false)]
    public override Guid Id { get; set; }

    /// <summary>
    ///     角色id
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    ///     机构id
    /// </summary>
    public Guid OrgId { get; set; }
    
    /// <summary>
    ///     机构信息
    /// </summary>
    [Navigate(nameof(OrgId))]
    public virtual SysOrgInfo OrgInfo { set; get; }
}