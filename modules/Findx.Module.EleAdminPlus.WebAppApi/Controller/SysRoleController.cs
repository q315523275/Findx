using System.ComponentModel;
using System.Security.Principal;
using Findx.AspNetCore.Mvc;
using Findx.Caching;
using Findx.Data;
using Findx.Extensions;
using Findx.Mapping;
using Findx.Module.EleAdminPlus.Shared.Models;
using Findx.Module.EleAdminPlus.WebAppApi.Dtos.Role;
using Findx.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.WebAppApi.Controller;

/// <summary>
///     角色服务
/// </summary>
[Area("system")]
[Route("api/[area]/role")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-角色"), Description("系统-角色")]
public class SysRoleController : CrudControllerBase<SysRoleInfo, RoleDto, RoleSaveDto, RolePageQueryDto, long, long>
{
    private readonly ICache _cache;
    private readonly string _cacheKey;
    
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cacheFactory"></param>
    public SysRoleController(ICurrentUser currentUser, ICacheFactory cacheFactory)
    {
        _cacheKey = $"EleAdminPlus:Role:{currentUser.TenantId}";
        _cache = cacheFactory.Create(CacheType.DefaultMemory);
    }
    
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

        var model = ToModelFromCreateRequest(req);
        model.CheckCreatedTime();
        model.CheckCreationAudited<SysRoleInfo, long>(HttpContext.User);
        model.CheckTenant(HttpContext.User);
        model.SetEmptyKey();
        
        await AddBeforeAsync(model, req, cancellationToken);
        var result = await repo.InsertAsync(model, cancellationToken);
        await AddAfterAsync(model, req, result, cancellationToken);
        
        var menuList = req.MenuIds.Select(x => new SysRoleMenuInfo { Id = keyGenerator.Create(), MenuId = x, RoleId = model.Id });
        if (menuList.Any()) await roleMenuRepo.InsertAsync(menuList, cancellationToken);
        
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
        
        var model = await repo.GetAsync(req.Id, cancellationToken);
        if (model == null) 
            return CommonResult.Fail("not.exist", "未能查到相关信息");
        
        repo.Attach(model.Clone().As<SysRoleInfo>());
        model = ToModelFromUpdateRequest(req, model);
        model.CheckUpdateTime();
        model.CheckUpdateAudited<SysRoleInfo, long>(HttpContext.User);
        model.CheckTenant(HttpContext.User);
        
        await EditBeforeAsync(model, req, cancellationToken);
        var res = await repo.SaveAsync(model, cancellationToken: cancellationToken);
        await EditAfterAsync(model, req, res, cancellationToken);
        
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
        
        var model = await repo.GetAsync(req.Id, cancellationToken);
        if (model == null) return CommonResult.Fail("not.exist", "未能查到相关信息");
        
        repo.Attach(model.Clone().As<SysRoleInfo>());
        model = req.MapTo(model);
        model.CheckUpdateAudited<SysRoleInfo, long>(HttpContext.User);
        model.OrgJson = req.OrgIds.ToJson();
        await repo.SaveAsync(model, cancellationToken: cancellationToken);
        
        var orgList = req.OrgIds.Select(x => new SysRoleOrgInfo { Id = keyGenerator.Create(), OrgId = x, RoleId = model.Id });
        await roleOrgRepo.DeleteAsync(x => x.RoleId == model.Id, cancellationToken);
        if (orgList.Any()) await roleOrgRepo.InsertAsync(orgList, cancellationToken);

        await _cache.RemoveAsync(_cacheKey, cancellationToken);
        
        return CommonResult.Success();
    }

    /// <summary>
    ///     添加完成之后
    /// </summary>
    /// <param name="model"></param>
    /// <param name="request"></param>
    /// <param name="result"></param>
    /// <param name="cancellationToken"></param>
    protected override async Task AddAfterAsync(SysRoleInfo model, RoleSaveDto request, int result, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(_cacheKey, cancellationToken);
    }

    /// <summary>
    ///     编辑完成之后
    /// </summary>
    /// <param name="model"></param>
    /// <param name="request"></param>
    /// <param name="result"></param>
    /// <param name="cancellationToken"></param>
    protected override async Task EditAfterAsync(SysRoleInfo model, RoleSaveDto request, int result, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(_cacheKey, cancellationToken);
    }

    /// <summary>
    ///     删除完成之后
    /// </summary>
    /// <param name="req"></param>
    /// <param name="total"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task DeleteAfterAsync(List<long> req, int total, CancellationToken cancellationToken = default)
    {
        var roleMenuRepo = GetRepository<SysRoleMenuInfo, long>();
        var roleOrgRepo = GetRepository<SysRoleOrgInfo, long>();

        await roleMenuRepo.DeleteAsync(x => req.Contains(x.RoleId), cancellationToken);
        await roleOrgRepo.DeleteAsync(x => req.Contains(x.RoleId), cancellationToken);
        
        await _cache.RemoveAsync(_cacheKey, cancellationToken);
    }
}