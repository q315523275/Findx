using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Exceptions;
using Findx.Module.EleAdminPlus.Shared.Models;
using Findx.Module.EleAdminPlus.WebAppApi.Dtos.Menu;
using Findx.Module.EleAdminPlus.WebAppApi.Vos.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.WebAppApi.Controller;

/// <summary>
///     菜单服务
/// </summary>
[Area("system")]
[Route("api/[area]/menu")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-菜单"), Description("系统-菜单")]
public class SysMenuController : CrudControllerBase<SysMenuInfo, MenuSimplifyDto, MenuAddOrEditDto, MenuPageQueryDto, long, long>
{
    /// <summary>
    ///     列表查询
    /// </summary>
    /// <param name="menuQueryDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<CommonResult<List<MenuSimplifyDto>>> ListAsync(MenuPageQueryDto menuQueryDto, CancellationToken cancellationToken = default)
    {
        menuQueryDto.PageSize = 9999;
        return base.ListAsync(menuQueryDto, cancellationToken);
    }

    /// <summary>
    ///     删除前校验
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task DeleteBeforeAsync(List<long> req, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysMenuInfo, long>();
        var isExist = await repo.ExistAsync(x => req.Contains(x.ParentId), cancellationToken);
        if (isExist) throw new FindxException("500", "请先删除子集菜单,再删除选中菜单");
    }
}