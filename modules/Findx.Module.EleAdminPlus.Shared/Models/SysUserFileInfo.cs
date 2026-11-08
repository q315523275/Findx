using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdminPlus.Shared.Models;

/// <summary>
///     用户文件
/// </summary>
[Table(Name = "sys_user_file")]
[EntityExtension(DataSource = "system")]
public class SysUserFileInfo : FullAuditedBase<long, long>, ITenant<long>, ISoftDeletable
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
    ///     文件名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     是否是文件夹, 0否, 1是
    /// </summary>
    public int IsDirectory { get; set; }

    /// <summary>
    ///     上级id
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    ///     文件路径
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    ///     文件大小
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    ///     文件类型
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    ///     租户ID（用于业务数据隔离）
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    ///     是否删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    ///     删除时间
    /// </summary>
    public DateTime? DeletionTime { get; set; }
}
