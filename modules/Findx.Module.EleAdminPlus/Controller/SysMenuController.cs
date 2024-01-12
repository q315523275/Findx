using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Linq;
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
[Description("系统-菜单")]
[ApiExplorerSettings(GroupName = "eleAdmin")]
[Tags("系统-菜单")]
public class SysMenuController : CrudControllerBase<SysMenuInfo, SetMenuRequest, QueryMenuRequest, long, long>
{
    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override Expression<Func<SysMenuInfo, bool>> CreatePageWhereExpression(QueryMenuRequest request)
    {
        var whereExp = PredicateBuilder.New<SysMenuInfo>()
                                       .AndIf(!request.Title.IsNullOrWhiteSpace(), x => x.Title.Contains(request.Title))
                                       .AndIf(!request.Path.IsNullOrWhiteSpace(), x => x.Path.Contains(request.Path))
                                       .AndIf(request.ParentId.HasValue, x => x.ParentId == request.ParentId)
                                       .AndIf(!request.Authority.IsNullOrWhiteSpace(), x => x.Authority.Contains(request.Authority))
                                       .Build();
        return whereExp;
    }

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