using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdminPlus.Models;

/// <summary>
///     角色菜单
/// </summary>
[Table(Name = "sys_role_menu")]
[EntityExtension(DataSource = "system")]
public class SysRoleMenuInfo : EntityBase<long>, ITenant
{
    /// <summary>
    ///     主键id
    /// </summary>
    [Column(IsPrimary = true, IsIdentity = true)]
    public override long Id { get; set; }

    /// <summary>
    ///     角色id
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    ///     菜单id
    /// </summary>
    public long MenuId { get; set; }

    /// <summary>
    ///     租户id
    /// </summary>
    public Guid? TenantId { get; set; }
    
    /// <summary>
    ///     角色信息
    /// </summary>
    [Navigate(nameof(MenuId))]
    public virtual SysMenuInfo MenuInfo { set; get; }
}