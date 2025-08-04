using System;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Expressions;

namespace Findx.WebHost.Model;

public class QueryNewsDto: PageBase
{
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Title { set; get; }
    
    [QueryField(FilterOperate = FilterOperate.Equal, Name = "Author")]
    public string Author { set; get; }
    
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public int? Status  { set; get; }
    
    /// <summary>
    ///     创建时间
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public DateTime? CreatedTime { get; set; }
}