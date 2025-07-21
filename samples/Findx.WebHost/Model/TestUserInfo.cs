using System;
using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.WebHost.Model;

public class TestUserInfo : ISoftDeletable, IEntity<int>
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public DateTime? DeletionTime { get; set; }
    public bool IsDeleted { get; set; }
    
    public object Clone()
    {
        return this.MemberwiseClone();
    }
}