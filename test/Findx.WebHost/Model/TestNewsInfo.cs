using System;
using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.WebHost.Model;

public class TestNewsInfo : EntityBase<int>, IExtraObject, IRequest<int>, IResponse
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public override int Id { get; set; }

    public string Title { get; set; }
    
    public string Author { get; set; }
    
    public string Content { get; set; }
    
    public int Status { get; set; }
    public string ExtraProperties { get; set; }
    
    /// <summary>
    ///     创建时间
    /// </summary>
    [Column(Name = "CreationTime")]
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    ///     创建人编号
    /// </summary>
    [Column(Name = "CreatorId")]
    public Guid? CreatorId { get; set; }

    /// <summary>
    ///     最后修改时间
    /// </summary>
    [Column(Name = "LastModificationTime")]
    public DateTime? LastUpdatedTime { get; set; }

    /// <summary>
    ///     最后修改人编号
    /// </summary>
    [Column(Name = "LastModifierId")]
    public Guid? LastUpdaterId { get; set; }
}