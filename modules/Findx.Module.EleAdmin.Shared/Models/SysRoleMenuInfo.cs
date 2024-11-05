using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdmin.Shared.Models;

/// <summary>
///     角色菜单
/// </summary>
[Table(Name = "sys_role_menu")]
[EntityExtension(DataSource = "system")]
public class SysRoleMenuInfo : EntityBase<Guid>
{
    /// <summary>
    ///     主键id
    /// </summary>
    [Column(IsPrimary = true, IsIdentity = true)]
    public override Guid Id { get; set; }

    /// <summary>
    ///     角色id
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    ///     菜单id
    /// </summary>
    public Guid MenuId { get; set; }

    /// <summary>
    ///     角色信息
    /// </summary>
    [Navigate(nameof(MenuId))]
    public virtual SysMenuInfo MenuInfo { set; get; }
}