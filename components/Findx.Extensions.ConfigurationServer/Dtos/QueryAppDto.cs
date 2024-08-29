using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Expressions;

namespace Findx.Extensions.ConfigurationServer.Dtos;

/// <summary>
///     查询App Dto
/// </summary>
public class QueryAppDto: PageBase
{
    /// <summary>
    ///     应用名称
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Name { get; set; }

    /// <summary>
    ///     appid
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public string AppId { get; set; }
}