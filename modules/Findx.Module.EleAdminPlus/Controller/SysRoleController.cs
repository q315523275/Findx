using System.ComponentModel;
using System.Security.Principal;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Mapping;
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
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-角色"), Description("系统-角色")]
public class SysRoleController : CrudControllerBase<SysRoleInfo, SetRoleRequest, QueryRoleRequest, long, long>
{
    /// <summary>
    ///     查询角色对应菜单
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    [HttpGet("menu/{roleId}"), Description("系统-查看角色菜单")]
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
    ///     查询角色对应机构
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    [HttpGet("org/{roleId}"), Description("系统-查看角色对应机构")]
    public CommonResult Org(long roleId)
    {
        var repo = GetRepository<SysRoleOrgInfo, long>();
        var orgRepo = GetRepository<SysOrgInfo, long>();

        var orgIdArray = repo.Select(x => x.RoleId == roleId, x => x.OrgId).Distinct();
        var orgList = orgRepo.Select<RoleOrgDto>();

        orgList.ForEach(x => { x.Checked = orgIdArray.Contains(x.Id); });

        return CommonResult.Success(orgList.OrderBy(x => x.Sort));
    }

    /// <summary>添加数据</summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("add"), Description("新增")]
    public override async Task<CommonResult> AddAsync([FromBody] SetRoleRequest req, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysRoleInfo, long>();
        var roleMenuRepo = GetRepository<SysRoleMenuInfo, long>();
        var keyGenerator = GetRequiredService<IKeyGenerator<long>>();
        var service = GetService<IPrincipal>();

        var model = ToModelFromCreateRequest(req);
        model.CheckCreatedTime();
        model.CheckCreationAudited<SysRoleInfo, long>(service);
        model.CheckTenant(service);
        model.SetEmptyKey();

        var menuList = req.MenuIds.Select(x => new SysRoleMenuInfo { Id = keyGenerator.Create(), MenuId = x, RoleId = model.Id });
        
        await repo.InsertAsync(model, cancellationToken);
        if (menuList.Any()) await roleMenuRepo.InsertAsync(menuList, cancellationToken);
        
        return CommonResult.Success();
    }

    /// <summary>编辑</summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("edit"), Description("编辑")]
    public override async Task<CommonResult> EditAsync([FromBody] SetRoleRequest req, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysRoleInfo, long>();
        var roleMenuRepo = GetRepository<SysRoleMenuInfo, long>();
        var keyGenerator = GetRequiredService<IKeyGenerator<long>>();
        var service = GetService<IPrincipal>();

        var model = ToModelFromUpdateRequest(req);
        model.CheckUpdateTime();
        model.CheckUpdateAudited<SysRoleInfo, long>(service);
        
        var menuList = req.MenuIds.Select(x => new SysRoleMenuInfo { Id = keyGenerator.Create(), MenuId = x, RoleId = model.Id });
        
        await repo.UpdateAsync(model, ignoreNullColumns: true, cancellationToken: cancellationToken);
        await roleMenuRepo.DeleteAsync(x => x.RoleId == model.Id, cancellationToken);
        if (menuList.Any()) await roleMenuRepo.InsertAsync(menuList, cancellationToken);
        
        return CommonResult.Success();
    }
    
    /// <summary>数据范围</summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("dataScope"), Description("数据范围")]
    public async Task<CommonResult> DataScopeAsync([FromBody] SetRoleOrgRequest req, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysRoleInfo, long>();
        var roleOrgRepo = GetRepository<SysRoleOrgInfo, long>();
        var keyGenerator = GetRequiredService<IKeyGenerator<long>>();
        var service = GetService<IPrincipal>();
        
        var model = req.MapTo<SysRoleInfo>();
        model.CheckUpdateTime();
        model.CheckUpdateAudited<SysRoleInfo, long>(service);

        var orgList = req.OrgIds.Select(x => new SysRoleOrgInfo { Id = keyGenerator.Create(), OrgId = x, RoleId = model.Id });
        
        await repo.UpdateAsync(model, updateColumns: x => new { x.DataScope, x.LastUpdatedTime, x.LastUpdaterId }, cancellationToken: cancellationToken);
        await roleOrgRepo.DeleteAsync(x => x.RoleId == model.Id, cancellationToken);
        if (orgList.Any()) await roleOrgRepo.InsertAsync(orgList, cancellationToken);
        
        return CommonResult.Success();
    }
}