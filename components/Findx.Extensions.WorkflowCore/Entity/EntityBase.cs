using Findx.Data;

namespace Findx.Extensions.WorkflowCore.Entity;

/// <summary>
///     实体基类
/// </summary>
public abstract class EntityBase: EntityBase<long>, IFullAudited<long>
{
    //  Id 沿用 EntityBase<long> 的虚属性，这里不要 override：
    //  FreeSql 按声明所在类型解析主键元数据，重写会让主键映射丢失；
    //  主键由 IKeyGenerator<long>（雪花）在插入前生成，不依赖数据库自增

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