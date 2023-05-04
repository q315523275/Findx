using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdmin.Models;

/// <summary>
///     租户
/// </summary>
[Table(Name = "sys_tenant")]
[EntityExtension(DataSource = "system")]
public class SysTenantInfo : FullAuditedBase<int, int>, ISoftDeletable
{
    /// <summary>
    ///     租户id
    /// </summary>
    [Column(IsPrimary = true, IsIdentity = true)]
    public override int Id { get; set; }

    /// <summary>
    ///     租户名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     备注
    /// </summary>
    public string Comments { get; set; }

    /// <summary>
    ///     是否删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    ///     删除时间
    /// </summary>
    public DateTime? DeletionTime { get; set; }
}