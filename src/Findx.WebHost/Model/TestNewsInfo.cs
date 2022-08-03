using FreeSql.DataAnnotations;

namespace Findx.WebHost.Model
{
    public class TestNewsInfo
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
    }
}
