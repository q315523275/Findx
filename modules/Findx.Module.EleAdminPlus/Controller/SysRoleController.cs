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
///     角色服务
/// </summary>
[Area("system")]
[Route("api/[area]/role")]
[Authorize]
[Description("系统-角色")]
[ApiExplorerSettings(GroupName = "eleAdmin")]
[Tags("系统-角色")]
public class SysRoleController : CrudControllerBase<SysRoleInfo, SetRoleRequest, QueryRoleRequest, long, long>
{
    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override Expression<Func<SysRoleInfo, bool>> CreatePageWhereExpression(QueryRoleRequest request)
    {
        var whereExp = PredicateBuilder.New<SysRoleInfo>()
                                       .AndIf(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name))
                                       .AndIf(!request.Code.IsNullOrWhiteSpace(), x => x.Code.Contains(request.Code))
                                       .AndIf(!request.Comments.IsNullOrWhiteSpace(), x => x.Comments.Contains(request.Comments))
                                       .Build();
        return whereExp;
    }

    /// <summary>
    ///     查询角色对应菜单
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    [HttpGet("menu/{roleId}")]
    [Description("系统-查看角色菜单")]
    public CommonResult Menu(long roleId)
    {
        var repo = GetRepository<SysRoleMenuInfo, long>();
        var menuRepo = GetRepository<SysMenuInfo, long>();

        var menuIdArray = repo.Select(x => x.RoleId == roleId, x => x.MenuId).Distinct();
        var menuList = menuRepo.Select<RoleMenuDto>();

        menuList.ForEach(x => { x.Checked = menuIdArray.Contains(x.Id); });

        return CommonResult.Success(menuList.OrderBy(x => x.Sort));
    }

    /// <summary>
    ///     设置角色对应菜单
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPut("menu/{roleId}")]
    [Description("系统-设置角色菜单")]
    public async Task<CommonResult> MenuAsync(long roleId, [FromBody] List<long> req)
    {
        var keyGenerator = GetRequiredService<IKeyGenerator<long>>();
        var repo = GetRepository<SysRoleMenuInfo, long>();
        await repo.DeleteAsync(x => x.RoleId == roleId);
        var list = req.Select(x => new SysRoleMenuInfo { Id = keyGenerator.Create(), MenuId = x, RoleId = roleId, TenantId = TenantManager.Current });
        // ReSharper disable once PossibleMultipleEnumeration
        if (list.Any()) 
            await repo.InsertAsync(list);

        return CommonResult.Success();
    }
}