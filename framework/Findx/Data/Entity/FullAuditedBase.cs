namespace Findx.Data;

/// <summary>
///     实体类审计基类
/// </summary>
/// <typeparam name="TUserKey"></typeparam>
public abstract class FullAuditedBase<TUserKey> : IFullAudited<TUserKey> where TUserKey : struct
{
    /// <summary>
    ///     创建人编号
    /// </summary>
    public virtual TUserKey? CreatorId { get; set; }

    /// <summary>
    ///     创建人
    /// </summary>
    public virtual string Creator { get; set; }
    
    /// <summary>
    ///     创建时间
    /// </summary>
    public virtual DateTime? CreatedTime { get; set; }

    /// <summary>
    ///     最后更新人编号
    /// </summary>
    public virtual TUserKey? LastUpdaterId { get; set; }

    /// <summary>
    ///     最后更新人
    /// </summary>
    public virtual string LastUpdater { get; set; }
    
    /// <summary>
    ///     最后更新时间
    /// </summary>
    public virtual DateTime? LastUpdatedTime { get; set; }
}

/// <summary>
///     实体类审计基类,包含主键ID
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TUserKey"></typeparam>
public abstract class FullAuditedBase<TKey, TUserKey> : EntityBase<TKey>, IFullAudited<TUserKey> where TUserKey : struct where TKey : IEquatable<TKey>
{
    /// <summary>
    ///     创建人编号
    /// </summary>
    public virtual TUserKey? CreatorId { get; set; }

    /// <summary>
    ///     创建人
    /// </summary>
    public string Creator { get; set; }

    /// <summary>
    ///     创建时间
    /// </summary>
    public virtual DateTime? CreatedTime { get; set; }

    /// <summary>
    ///     最后更新人编号
    /// </summary>
    public virtual TUserKey? LastUpdaterId { get; set; }

    /// <summary>
    ///     最后更新人
    /// </summary>
    public virtual string LastUpdater { get; set; }

    /// <summary>
    ///     最后更新时间
    /// </summary>
    public virtual DateTime? LastUpdatedTime { get; set; }
}