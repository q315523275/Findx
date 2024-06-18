using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Linq;

namespace Findx.WebHost.Model;

public class QueryNewsDto: PageBase
{
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Title { set; get; }
    
    [QueryField(FilterOperate = FilterOperate.Equal, Name = "Author")]
    public string Author { set; get; }
}