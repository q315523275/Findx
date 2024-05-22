using System;
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
    public object Tables([FromServices] IFreeSql fsql)
    {
        return fsql.Ado.Query<dynamic>("SHOW TABLES");
    }
    
    [HttpGet("/database/columns")]
    public object Columns([FromServices] IFreeSql fsql)
    {
        return fsql.Ado.Query<dynamic>("DESC sys_user");
    }
}