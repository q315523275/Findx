using System.ComponentModel;
using System.Linq.Expressions;
using System.Security.Principal;
using Findx.AspNetCore.Mvc;
using Findx.Caching;
using Findx.Data;
using Findx.Expressions;
using Findx.Extensions;
using Findx.Mapping;
using Findx.Module.EleAdmin.Dtos.Org;
using Findx.Module.EleAdmin.Dtos.Role;
using Findx.Module.EleAdmin.Shared.Models;
using Findx.Security;
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
[ApiExplorerSettings(GroupName = "eleAdmin")]
[Tags("系统-角色")]
[Description("系统-角色")]
public class SysRoleController : CrudControllerBase<SysRoleInfo, RoleSaveDto, RoleQueryDto, Guid, Guid>
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
        _cacheKey = $"EleAdmin:Role:{currentUser.TenantId}";
        _cache = cacheFactory.Create(CacheType.DefaultMemory);
    }

    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override Expression<Func<SysRoleInfo, bool>> CreateWhereExpression(RoleQueryDto request)
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
            ? menuRepo.Select<RoleMenuSimplifyDto>()
            : menuRepo.Select<RoleMenuSimplifyDto>(x => x.ApplicationCode == applicationCode);

        menuList.ForEach(x =>
        {
            if (menuIdArray.Contains(x.Id)) x.Checked = true;
        });

        return CommonResult.Success(menuList.OrderBy(x => x.Sort));
    }

    /// <summary>
    ///     查询角色对应机构
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("org/{roleId}"), Description("系统-查看角色对应机构")]
    public async Task<CommonResult<IOrderedEnumerable<RoleOrgDto>>> OrgAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var repo = GetRepository<SysRoleOrgInfo, Guid>();
        var orgRepo = GetRepository<SysOrgInfo, Guid>();

        var orgIdArray = (await repo.SelectAsync(x => x.RoleId == roleId, x => x.OrgId, cancellationToken: cancellationToken)).Distinct();
        var orgList = await orgRepo.SelectAsync<RoleOrgDto>(cancellationToken: cancellationToken);

        orgList.ForEach(x => { x.Checked = orgIdArray.Contains(x.Id); });

        return CommonResult.Success(orgList.OrderBy(x => x.Sort));
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

    /// <summary>数据范围</summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("dataScope"), Description("数据范围")]
    public async Task<CommonResult> DataScopeAsync([FromBody] RoleOrgSaveDto req, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysRoleInfo, Guid>();
        var roleOrgRepo = GetRepository<SysRoleOrgInfo, Guid>();
        var keyGenerator = GetRequiredService<IKeyGenerator<Guid>>();
        var service = GetService<IPrincipal>();
        
        var model = await repo.GetAsync(req.Id, cancellationToken);
        if (model == null) return CommonResult.Fail("not.exist", "未能查到相关信息");
        repo.Attach(model.Clone().As<SysRoleInfo>());
        model = req.MapTo(model);
        model.CheckUpdateAudited<SysRoleInfo, long>(service);
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
    protected override async Task AddAfterAsync(SysRoleInfo model, RoleSaveDto request, int result)
    {
        await _cache.RemoveAsync(_cacheKey);
    }

    /// <summary>
    ///     编辑完成之后
    /// </summary>
    /// <param name="model"></param>
    /// <param name="request"></param>
    /// <param name="result"></param>
    protected override async Task EditAfterAsync(SysRoleInfo model, RoleSaveDto request, int result)
    {
        await _cache.RemoveAsync(_cacheKey);
    }

    /// <summary>
    ///     删除完成之后
    /// </summary>
    /// <param name="req"></param>
    /// <param name="total"></param>
    /// <returns></returns>
    protected override async Task DeleteAfterAsync(List<Guid> req, int total)
    {
        await _cache.RemoveAsync(_cacheKey);
    }
}