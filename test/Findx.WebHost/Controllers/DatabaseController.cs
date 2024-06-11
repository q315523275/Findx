using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Findx.Data;
using Findx.FreeSql;
using Findx.WebHost.Model;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
///     Database
/// </summary>
public class DatabaseController: Controller
{
    [HttpGet("/database/tables")]
    public object Tables([FromServices] IFreeSql fsql, string name)
    {
        return fsql.DbFirst.GetTablesByDatabase(name).Select(x => new { x.Id, x.Name, x.Comment, x.Schema });
    }
    
    [HttpGet("/database/columns")]
    public object Columns([FromServices] IFreeSql fsql, string name)
    {
        var table = fsql.DbFirst.GetTableByName(name);
        return table.Columns.Select(x => new { x.Comment, x.Name, x.DbTypeText, x.IsPrimary, x.IsNullable, x.DefaultValue, x.Position }).OrderBy(x => x.Position);
    }
}