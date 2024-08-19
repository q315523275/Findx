using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Linq;
using Findx.Module.EleAdmin.Dtos;
using Findx.Module.EleAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Controller;

/// <summary>
///     角色服务
/// </summary>
[Area("system")]
[Route("api/[area]/role")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdmin"), Tags("系统-角色"), Description("系统-角色")]
public class SysRoleController : CrudControllerBase<SysRoleInfo, SetRoleRequest, QueryRoleRequest, Guid, Guid>
{
    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override Expression<Func<SysRoleInfo, bool>> CreateWhereExpression(QueryRoleRequest request)
    {
        var whereExp = PredicateBuilder.New<SysRoleInfo>()
                                       .AndIf(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name))
                                       .AndIf(!request.Code.IsNullOrWhiteSpace(), x => x.Code.Contains(request.Code))
                                       .AndIf(!request.Comments.IsNullOrWhiteSpace(), x => x.Comments.Contains(request.Comments))
                                       .AndIf(!request.ApplicationCode.IsNullOrWhiteSpace(), x => x.ApplicationCode == request.ApplicationCode)
                                       .Build();
        return whereExp;
    }

    /// <summary>
    ///     查询角色对应菜单
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="applicationCode"></param>
    /// <returns></returns>
    [HttpGet("menu/{roleId}")]
    [Description("系统-查看角色菜单")]
    public CommonResult Menu(Guid roleId, string applicationCode)
    {
        var repo = GetRequiredService<IRepository<SysRoleMenuInfo>>();
        var menuRepo = GetRequiredService<IRepository<SysMenuInfo>>();

        var menuIdArray = repo.Select(x => x.RoleId == roleId, x => x.MenuId).Distinct();
        var menuList = applicationCode.IsNullOrWhiteSpace()
            ? menuRepo.Select<RoleMenuDto>()
            : menuRepo.Select<RoleMenuDto>(x => x.ApplicationCode == applicationCode);

        menuList.ForEach(x =>
        {
            if (menuIdArray.Contains(x.Id)) x.Checked = true;
        });

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
    public async Task<CommonResult> MenuAsync(Guid roleId, [FromBody] List<Guid> req)
    {
        var keyGenerator = GetRequiredService<IKeyGenerator<Guid>>();
        var repo = GetRequiredService<IRepository<SysRoleMenuInfo>>();
        await repo.DeleteAsync(x => x.RoleId == roleId);
        var list = req.Select(x => new SysRoleMenuInfo
        {
            Id = keyGenerator.Create(),
            MenuId = x,
            RoleId = roleId
        });
        // ReSharper disable once PossibleMultipleEnumeration
        if (list.Any()) await repo.InsertAsync(list);

        return CommonResult.Success();
    }
}