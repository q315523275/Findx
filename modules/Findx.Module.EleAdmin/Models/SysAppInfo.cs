using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdmin.Models;

/// <summary>
///     应用信息实体
/// </summary>
[Table(Name = "sys_app")]
[EntityExtension(DataSource = "system")]
public class SysAppInfo : FullAuditedBase<Guid, Guid>, ISoftDeletable, ITenant, ISort, IResponse
{
    /// <summary>
    ///     主键id
    /// </summary>
    [Column(Name = "id", IsPrimary = true)]
    public override Guid Id { get; set; }

    /// <summary>
    ///     编码
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     应用名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     状态（字典 0正常 1停用）
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    ///     是否删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    ///     删除时间
    /// </summary>
    public DateTime? DeletionTime { get; set; }

    /// <summary>
    ///     排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     租户
    /// </summary>
    public string TenantId { get; set; }
}