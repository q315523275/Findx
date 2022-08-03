using Findx.Data;
using FreeSql.DataAnnotations;
using System;

namespace Findx.WebHost.Model
{
    public class TestUserInfo : ISoftDeletable
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public DateTime? DeletionTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
