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
}