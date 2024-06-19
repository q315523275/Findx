using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Module.EleAdminPlus.Dtos;
using Findx.Module.EleAdminPlus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.Controller;

/// <summary>
///     菜单服务
/// </summary>
[Area("system")]
[Route("api/[area]/menu")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-菜单"), Description("系统-菜单")]
public class SysMenuController : CrudControllerBase<SysMenuInfo, SetMenuRequest, QueryMenuRequest, long, long>
{
    /// <summary>
    ///     列表查询
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<CommonResult<List<SysMenuInfo>>> ListAsync(QueryMenuRequest request, CancellationToken cancellationToken = default)
    {
        request.PageSize = 9999;
        return base.ListAsync(request, cancellationToken);
    }
}