using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.WebHost.Model;

public class TestNewsInfo : IEntity<int>, IExtraObject
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public string ExtraProperties { get; set; }
}