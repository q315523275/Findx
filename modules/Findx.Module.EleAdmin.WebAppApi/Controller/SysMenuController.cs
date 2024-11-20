using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Expressions;
using Findx.Extensions;
using Findx.Module.EleAdmin.Dtos.Menu;
using Findx.Module.EleAdmin.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Controller;

/// <summary>
///     菜单服务
/// </summary>
[Area("system")]
[Route("api/[area]/menu")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdmin"), Tags("系统-菜单"), Description("系统-菜单")]
public class SysMenuController : CrudControllerBase<SysMenuInfo, MenuSaveDto, MenuQueryDto, Guid, Guid>
{
    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override Expression<Func<SysMenuInfo, bool>> CreateWhereExpression(MenuQueryDto request)
    {
        var whereExp = PredicateBuilder.New<SysMenuInfo>()
            .AndIf(!request.Title.IsNullOrWhiteSpace(), x => x.Title.Contains(request.Title))
            .AndIf(!request.Path.IsNullOrWhiteSpace(), x => x.Path.Contains(request.Path))
            .AndIf(request.ParentId.HasValue, x => x.ParentId == request.ParentId)
            .AndIf(!request.Authority.IsNullOrWhiteSpace(), x => x.Authority.Contains(request.Authority))
            .AndIf(!request.ApplicationCode.IsNullOrWhiteSpace(), x => x.ApplicationCode == request.ApplicationCode)
            .Build();
        return whereExp;
    }

    /// <summary>
    ///     列表查询
    /// </summary>
    /// <param name="menuQueryDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<CommonResult<List<SysMenuInfo>>> ListAsync(MenuQueryDto menuQueryDto, CancellationToken cancellationToken = default)
    {
        menuQueryDto.PageSize = 9999;
        return base.ListAsync(menuQueryDto, cancellationToken);
    }
}