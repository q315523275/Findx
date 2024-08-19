using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdminPlus.Shared.Models;

/// <summary>
///     系统用户角色
/// </summary>
[Table(Name = "sys_user_role")]
[EntityExtension(DataSource = "system")]
public class SysUserRoleInfo : EntityBase<long>
{
    /// <summary>
    ///     主键id
    /// </summary>
    [Column(IsPrimary = true, IsIdentity = false)]
    public override long Id { get; set; }

    /// <summary>
    ///     用户id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    ///     角色id
    /// </summary>
    public long RoleId { get; set; }
    
    /// <summary>
    ///     角色信息
    /// </summary>
    [Navigate(nameof(RoleId))]
    public virtual SysRoleInfo RoleInfo { set; get; }
}