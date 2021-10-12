using Findx.Data;
using FreeSql.DataAnnotations;
using SqlSugar;
using System;

namespace Findx.WebHost.Model
{
    public class TestUserInfo : ISoftDeletable
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool Deleted { get; set; }
    }
}
