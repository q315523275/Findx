using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdminPlus.Shared.Models;

/// <summary>
///     字典数据信息表
/// </summary>
[Table(Name = "sys_dictionary_data")]
[EntityExtension(DataSource = "system")]
public class SysDictionaryDataInfo : FullAuditedBase<long, long>, ISort, ISoftDeletable
{
    /// <summary>
    ///     字典项id
    /// </summary>
    [Column(IsPrimary = true)]
    public override long Id { get; set; }

    /// <summary>
    ///     字典id
    /// </summary>
    public long DictId { get; set; }

    /// <summary>
    ///     字典项名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     字典项值
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///     排序号
    /// </summary>
    public int Sort { get; set; }
    
    /// <summary>
    ///     所属应用ID（冗余字段，方便查询）
    /// </summary>
    public long? AppId { get; set; }
    
    /// <summary>
    ///     租户ID（冗余字段，方便查询）
    /// </summary>
    public long? TenantId { get; set; }
    
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
    
    /// <summary>
    ///     字典类型信息
    /// </summary>
    [Navigate(nameof(DictId))]
    public virtual SysDictionaryInfo DictInfo { get; set; }
}