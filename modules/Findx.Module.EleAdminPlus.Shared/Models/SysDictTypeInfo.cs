using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdminPlus.Shared.Models;

/// <summary>
///     字典类型
/// </summary>
[Table(Name = "sys_dict_type")]
[EntityExtension(DataSource = "system")]
public class SysDictTypeInfo : FullAuditedBase<long, long>, ISort, ISoftDeletable
{
    /// <summary>
    ///     字典id
    /// </summary>
    [Column(IsPrimary = true)]
    public override long Id { get; set; }

    /// <summary>
    ///     字典标识
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     字典名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     排序号
    /// </summary>
    public int Sort { get; set; }
    
    /// <summary>
    ///     所属应用ID（NULL表示系统级或租户级）
    /// </summary>
    public long? AppId { get; set; }
    
    /// <summary>
    ///     租户ID（NULL表示系统级或应用级）
    /// </summary>
    public long? TenantId { get; set; }
    
    /// <summary>
    ///     字典级别：0系统级 1应用级 2租户级
    /// </summary>
    public int DictLevel { get; set; } = 2;
    
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