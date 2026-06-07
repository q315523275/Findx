using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdminPlus.Shared.Models;

/// <summary>
///     应用/平台 - Findx提供的功能模块
/// </summary>
[Table(Name = "sys_app")]
[EntityExtension(DataSource = "system")]
public class SysAppInfo : FullAuditedBase<long, long>, ISoftDeletable
{
    /// <summary>
    ///     应用id
    /// </summary>
    [Column(IsPrimary = true)]
    public override long Id { get; set; }

    /// <summary>
    ///     应用编码（唯一）
    /// </summary>
    public string AppCode { get; set; }

    /// <summary>
    ///     应用名称
    /// </summary>
    public string AppName { get; set; }

    /// <summary>
    ///     图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    ///     描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     是否启用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     是否删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    ///     删除时间
    /// </summary>
    public DateTime? DeletionTime { get; set; }
}
