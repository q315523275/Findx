using Findx.Data;
using Findx.Module.EleAdminPlus.Enum;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdminPlus.Models;

/// <summary>
///     角色
/// </summary>
[Table(Name = "sys_role")]
[EntityExtension(DataSource = "system")]
public class SysRoleInfo : FullAuditedBase<long, long>, ISoftDeletable, ITenant
{
    /// <summary>
    ///     角色id
    /// </summary>
    [Column(IsPrimary = true, IsIdentity = false)]
    public override long Id { get; set; }

    /// <summary>
    ///     角色名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     角色标识
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     数据范围（1：全部数据权限 2：自定数据权限 3：本部门数据权限 4：本部门及以下数据权限 5:本人）
    /// </summary>
    [Column(MapType = typeof(int))]
    public DataScope DataScope { get; set; } = DataScope.All;
    
    /// <summary>
    ///     Ip限定
    /// </summary>
    public bool IpLimit { get; set; }
    
    /// <summary>
    ///     Ip地址
    /// </summary>
    public string IpAddress { get; set; }
    
    /// <summary>
    ///     备注
    /// </summary>
    public string Comments { get; set; }

    /// <summary>
    ///     租户id
    /// </summary>
    public Guid? TenantId { get; set; }
    
    /// <summary>
    ///     是否删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    ///     删除时间
    /// </summary>
    public DateTime? DeletionTime { get; set; }
}