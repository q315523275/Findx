using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Caching;
using Findx.Data;
using Findx.Exceptions;
using Findx.Expressions;
using Findx.Module.EleAdminPlus.Mvc.Filters;
using Findx.Module.EleAdminPlus.Shared.Enums;
using Findx.Module.EleAdminPlus.Shared.Models;
using Findx.Module.EleAdminPlus.Shared.ServiceDefaults;
using Findx.Module.EleAdminPlus.WebAppApi.Dtos.Organization;
using Findx.Module.EleAdminPlus.WebAppApi.Vos.Organization;
using Findx.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.WebAppApi.Controller;

/// <summary>
///     系统-组织
/// </summary>
[Area("system")]
[Route("api/[area]/organization")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-组织"), Description("系统-组织")]
public class SysOrganizationController : CrudControllerBase<SysOrganizationInfo, OrganizationSimplifyDto, OrganizationSaveDto, OrganizationPageQueryDto, long, long>
{
    private readonly IWorkContext _workContext;
    private readonly ICache _cache;
    private readonly string _cacheKey;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="workContext"></param>
    /// <param name="currentUser"></param>
    /// <param name="cacheFactory"></param>
    public SysOrganizationController(IWorkContext workContext, ICurrentUser currentUser, ICacheFactory cacheFactory)
    {
        _workContext = workContext;
        
        _cacheKey = $"EleAdminPlus:Organization:{currentUser.TenantId}";
        _cache = cacheFactory.Create(CacheType.DefaultMemory);
    }

    /// <summary>
    ///     构建数据范围表达式
    /// </summary>
    /// <param name="defaultWhere"></param>
    /// <returns></returns>
    private Expression<Func<SysOrganizationInfo, bool>> BuildDataScopeWhereExpression(Expression<Func<SysOrganizationInfo, bool>> defaultWhere)
    {
        var user = _workContext.ContextUser;
        var exp = PredicateBuilder.New<SysOrganizationInfo>()
                                  .AndIf(_workContext.DataScope is DataScope.Custom, x => _workContext.OrgIds.Contains(x.Id))
                                  .AndIf(_workContext.DataScope is DataScope.Subs, x => _workContext.OrgIds.Contains(x.Id))
                                  .AndIf(_workContext.DataScope is DataScope.Department, x => x.Id == user.OrgId.Value)
                                  .AndIf(_workContext.DataScope is DataScope.Oneself, x => x.Id == user.OrgId.Value);
        return defaultWhere == null ? exp.Build() : exp.And(defaultWhere).Build();
    }

    /// <summary>
    ///     构建筛选表达式
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override Expression<Func<SysOrganizationInfo, bool>> CreateWhereExpression(OrganizationPageQueryDto request)
    {
        return BuildDataScopeWhereExpression(base.CreateWhereExpression(request));
    }

    /// <summary>
    ///     分页查询
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [DataScopeLimiter, IpAddressLimiter]
    public override Task<CommonResult<PageResult<List<OrganizationSimplifyDto>>>> PageAsync(OrganizationPageQueryDto request, CancellationToken cancellationToken = default)
    {
        return base.PageAsync(request, cancellationToken);
    }

    /// <summary>
    ///     列表查询
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [DataScopeLimiter, IpAddressLimiter]
    public override Task<CommonResult<List<OrganizationSimplifyDto>>> ListAsync(OrganizationPageQueryDto request, CancellationToken cancellationToken = default)
    {
        return base.ListAsync(request, cancellationToken);
    }

    /// <summary>
    ///     删除前校验
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task DeleteBeforeAsync(List<long> req, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysOrganizationInfo, long>();
        var isExist = await repo.ExistAsync(x => req.Contains(x.ParentId), cancellationToken);
        if (isExist) throw new FindxException("500", "请先删除下属机构,再删除选中机构");
    }

    /// <summary>
    ///     添加完成之后
    /// </summary>
    /// <param name="model"></param>
    /// <param name="request"></param>
    /// <param name="result"></param>
    /// <param name="cancellationToken"></param>
    protected override async Task AddAfterAsync(SysOrganizationInfo model, OrganizationSaveDto request, int result, CancellationToken cancellationToken = default)
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
    protected override async Task EditAfterAsync(SysOrganizationInfo model, OrganizationSaveDto request, int result, CancellationToken cancellationToken = default)
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
        await _cache.RemoveAsync(_cacheKey, cancellationToken);
    }
}