using FreeSql.DataAnnotations;
using SqlSugar;

namespace Findx.WebHost.Model
{
    public class TestNewsInfo
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
    }
}
