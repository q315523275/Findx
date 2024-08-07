﻿using System.ComponentModel;
using System.Security.Principal;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Mapping;
using Findx.Module.EleAdminPlus.Dtos.Role;
using Findx.Module.EleAdminPlus.Models;
using Findx.Security;
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
public class SysRoleController : CrudControllerBase<SysRoleInfo, RoleDto, RoleSaveDto, RolePageQueryDto, long, long>
{
    /// <summary>
    ///     查询角色对应菜单
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("menu/{roleId}"), Description("系统-查看角色菜单")]
    public async Task<CommonResult<IOrderedEnumerable<RoleMenuDto>>> MenuAsync(long roleId, CancellationToken cancellationToken)
    {
        var repo = GetRepository<SysRoleMenuInfo, long>();
        var menuRepo = GetRepository<SysMenuInfo, long>();

        var menuIdArray = (await repo.SelectAsync(x => x.RoleId == roleId, x => x.MenuId, cancellationToken: cancellationToken)).Distinct();
        var menuList = await menuRepo.SelectAsync<RoleMenuDto>(cancellationToken: cancellationToken);
        
        menuList.ForEach(x => { x.Checked = menuIdArray.Contains(x.Id); });

        return CommonResult.Success(menuList.OrderBy(x => x.Sort));
    }

    /// <summary>
    ///     查询角色对应机构
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("org/{roleId}"), Description("系统-查看角色对应机构")]
    public async Task<CommonResult<IOrderedEnumerable<RoleOrgDto>>> OrgAsync(long roleId, CancellationToken cancellationToken)
    {
        var repo = GetRepository<SysRoleOrgInfo, long>();
        var orgRepo = GetRepository<SysOrgInfo, long>();

        var orgIdArray = (await repo.SelectAsync(x => x.RoleId == roleId, x => x.OrgId, cancellationToken: cancellationToken)).Distinct();
        var orgList = await orgRepo.SelectAsync<RoleOrgDto>(cancellationToken: cancellationToken);

        orgList.ForEach(x => { x.Checked = orgIdArray.Contains(x.Id); });

        return CommonResult.Success(orgList.OrderBy(x => x.Sort));
    }

    /// <summary>添加数据</summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("add"), Description("新增")]
    public override async Task<CommonResult> AddAsync([FromBody] RoleSaveDto req, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysRoleInfo, long>();
        var roleMenuRepo = GetRepository<SysRoleMenuInfo, long>();
        var keyGenerator = GetRequiredService<IKeyGenerator<long>>();
        var principal = GetService<IPrincipal>();

        var model = ToModelFromCreateRequest(req);
        model.CheckCreatedTime();
        model.CheckCreationAudited<SysRoleInfo, long>(principal);
        model.CheckTenant(principal);
        model.SetEmptyKey();

        var menuList = req.MenuIds.Select(x => new SysRoleMenuInfo { Id = keyGenerator.Create(), MenuId = x, RoleId = model.Id });
        
        await repo.InsertAsync(model, cancellationToken);
        
        if (menuList.Any()) 
            await roleMenuRepo.InsertAsync(menuList, cancellationToken);
        
        return CommonResult.Success();
    }

    /// <summary>编辑</summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("edit"), Description("编辑")]
    public override async Task<CommonResult> EditAsync([FromBody] RoleSaveDto req, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysRoleInfo, long>();
        var roleMenuRepo = GetRepository<SysRoleMenuInfo, long>();
        var keyGenerator = GetRequiredService<IKeyGenerator<long>>();
        var principal = GetService<IPrincipal>();
        
        var model = await repo.GetAsync(req.Id, cancellationToken);
        if (model == null) 
            return CommonResult.Fail("not.exist", "未能查到相关信息");
        
        repo.Attach(model.Clone().As<SysRoleInfo>());
        
        model = ToModelFromUpdateRequest(req, model);
        if (model is IUpdateTime entity1)
        {
            entity1.LastUpdatedTime = DateTime.Now;
        }
        if (model is IUpdateAudited<long> entity2)
        {
            entity2.LastUpdaterId = principal?.Identity.GetUserId<long>() ?? default;
            entity2.LastUpdatedTime = DateTime.Now; 
        }
        model.CheckTenant(principal);
        
        await EditBeforeAsync(model, req);
        var res = await repo.SaveAsync(model, cancellationToken: cancellationToken);
        await EditAfterAsync(model, req, res);
        
        var menuList = req.MenuIds.Select(x => new SysRoleMenuInfo { Id = keyGenerator.Create(), MenuId = x, RoleId = model.Id });
        await roleMenuRepo.DeleteAsync(x => x.RoleId == model.Id, cancellationToken);
        if (menuList.Any()) await roleMenuRepo.InsertAsync(menuList, cancellationToken);
        
        return CommonResult.Success();
    }
    
    /// <summary>数据范围</summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("dataScope"), Description("数据范围")]
    public async Task<CommonResult> DataScopeAsync([FromBody] RoleOrgSaveDto req, CancellationToken cancellationToken = default)
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
        if (orgList.Any()) 
            await roleOrgRepo.InsertAsync(orgList, cancellationToken);
        
        return CommonResult.Success();
    }
}