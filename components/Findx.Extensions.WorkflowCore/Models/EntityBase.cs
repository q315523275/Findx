using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;

namespace Findx.Extensions.WorkflowCore.Models;

/// <summary>
///     实体基类
/// </summary>
public abstract class EntityBase: EntityBase<long>, IFullAudited<long>
{
    /// <summary>
    ///     编号Id
    /// </summary>
    [Key]
    [Column(name: "id")]
    public override long Id { get; set; }

    /// <summary>
    ///     创建人
    /// </summary>
    public string Creator { get; set; }
    
    /// <summary>
    ///     创建人Id
    /// </summary>
    public long? CreatorId { get; set; }
    
    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }
    
    /// <summary>
    ///     更新人
    /// </summary>
    public string LastUpdater { get; set; }
    
    /// <summary>
    ///     更新人Id
    /// </summary>
    public long? LastUpdaterId { get; set; }
    
    /// <summary>
    ///     更新时间
    /// </summary>
    public DateTime? LastUpdatedTime { get; set; }
}