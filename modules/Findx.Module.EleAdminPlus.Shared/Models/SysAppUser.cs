using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdminPlus.Shared.Models;

/// <summary>
///     应用/平台成员 - 租户下的具体员工权限管理
/// </summary>
[Table(Name = "sys_app_member")]
[EntityExtension(DataSource = "system")]
public class SysAppUser : FullAuditedBase<long, long>, ITenant<long>
{
    /// <summary>
    ///     主键id
    /// </summary>
    [Column(IsPrimary = true)]
    public override long Id { get; set; }

    /// <summary>
    ///     租户ID
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    ///     应用ID
    /// </summary>
    public long AppId { get; set; }
    
    /// <summary>
    ///     用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    ///     状态: 0正常 1冻结
    /// </summary>
    public int Status { get; set; } = 1;
    
    /// <summary>
    ///     备注
    /// </summary>
    public string Comments { get; set; }
    
    /// <summary>
    ///     导航属性 - 租户信息
    /// </summary>
    [Navigate(nameof(TenantId))]
    public virtual SysTenantInfo TenantInfo { get; set; }

    /// <summary>
    ///     导航属性 - 用户信息
    /// </summary>
    [Navigate(nameof(UserId))]
    public virtual SysUserInfo UserInfo { get; set; }
}
