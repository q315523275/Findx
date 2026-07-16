using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdminPlus.Shared.Models;

/// <summary>
///     租户应用订阅
/// </summary>
[Table(Name = "sys_tenant_application_subscriptions")]
[EntityExtension(DataSource = "system")]
public class SysTenantAppSubscriptionInfo: EntityBase<long>
{
    /// <summary>
    ///     主键id
    /// </summary>
    [Column(IsPrimary = true)]
    public override long Id { get; set; }

    /// <summary>
    ///     租户ID
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    ///     应用ID
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    ///     订阅时间
    /// </summary>
    public DateTime SubscribeTime { get; set; }

    /// <summary>
    ///     到期时间
    /// </summary>
    public DateTime ExpireTime { get; set; }

    /// <summary>
    ///     状态：0未激活 1正常 2过期
    /// </summary>
    public int Status { get; set; }
    
    /// <summary>
    ///    导航属性 - 租户信息
    /// </summary>
    [Navigate(nameof(TenantId))]
    public virtual SysTenantInfo TenantInfo { get; set; }

    /// <summary>
    ///     导航属性 - 应用信息
    /// </summary>
    [Navigate(nameof(AppId))]
    public virtual SysAppInfo AppInfo { get; set; }
}
