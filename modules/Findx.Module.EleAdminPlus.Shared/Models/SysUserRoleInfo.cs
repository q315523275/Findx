using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdminPlus.Shared.Models;

/// <summary>
///     系统用户角色
/// </summary>
[Table(Name = "sys_user_roles")]
[EntityExtension(DataSource = "system")]
public class SysUserRoleInfo : EntityBase<long>, ITenant<long>
{
    /// <summary>
    ///     主键id
    /// </summary>
    [Column(IsPrimary = true)]
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
    ///     租户id
    /// </summary>
    public long? TenantId { get; set; }
    
    /// <summary>
    ///    导航属性 - 角色信息
    /// </summary>
    [Navigate(nameof(RoleId))]
    public virtual SysRoleInfo RoleInfo { set; get; }
}