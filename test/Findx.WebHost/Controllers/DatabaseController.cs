using System.ComponentModel;
using System.Linq;
using Findx.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
///     库表服务
/// </summary>
[Route("api/database")]
[Description("库表服务"), Tags("库表服务")]
public class DatabaseController: ApiControllerBase
{
    /// <summary>
    ///     表信息
    /// </summary>
    /// <param name="fsql"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpGet("tables")]
    public object Tables([FromServices] IFreeSql fsql, string name)
    {
        return fsql.DbFirst.GetTablesByDatabase(name).Select(x => new { x.Id, x.Name, x.Comment, x.Schema });
    }
    
    /// <summary>
    ///     字段信息
    /// </summary>
    /// <param name="fsql"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpGet("columns")]
    public object Columns([FromServices] IFreeSql fsql, string name)
    {
        var table = fsql.DbFirst.GetTableByName(name);
        return table.Columns.Select(x => new { x.Comment, x.Name, x.DbTypeText, x.IsPrimary, x.IsNullable, x.DefaultValue, x.Position }).OrderBy(x => x.Position);
    }
}