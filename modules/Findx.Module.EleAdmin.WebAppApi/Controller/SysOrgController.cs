using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Caching;
using Findx.Exceptions;
using Findx.Expressions;
using Findx.Extensions;
using Findx.Module.EleAdmin.Dtos.Org;
using Findx.Module.EleAdmin.Shared.Models;
using Findx.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Controller;

/// <summary>
///     机构服务
/// </summary>
[Area("system")]
[Route("api/[area]/org")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdmin"), Tags("系统-机构"), Description("系统-机构")]
public class SysOrgController : CrudControllerBase<SysOrgInfo, OrgSaveDto, OrgQueryDto, Guid, Guid>
{
    private readonly ICache _cache;
    private readonly string _cacheKey;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="cacheFactory"></param>
    public SysOrgController(ICurrentUser currentUser, ICacheFactory cacheFactory)
    {
        _cacheKey = $"EleAdmin:Org:{currentUser.TenantId}";
        _cache = cacheFactory.Create(CacheType.DefaultMemory);
    }

    
    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override Expression<Func<SysOrgInfo, bool>> CreateWhereExpression(OrgQueryDto request)
    {
        var whereExp = PredicateBuilder.New<SysOrgInfo>()
            .AndIf(request.Pid != null && request.Pid != Guid.Empty, x => x.ParentId == request.Pid)
            .AndIf(!request.Keywords.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Keywords))
            .Build();
        return whereExp;
    }

    /// <summary>
    ///     添加完成之后
    /// </summary>
    /// <param name="model"></param>
    /// <param name="request"></param>
    /// <param name="result"></param>
    /// <param name="cancellationToken"></param>
    protected override async Task AddAfterAsync(SysOrgInfo model, OrgSaveDto request, int result, CancellationToken cancellationToken = default)
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
    protected override async Task EditAfterAsync(SysOrgInfo model, OrgSaveDto request, int result, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(_cacheKey, cancellationToken);
    }

    /// <summary>
    ///     删除前校验
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task DeleteBeforeAsync(List<Guid> req, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysOrgInfo>();
        var isExist = await repo.ExistAsync(x => req.Contains(x.ParentId), cancellationToken);
        if (isExist) throw new FindxException("500", "请先删除下属机构,再删除选中机构");
    }

    /// <summary>
    ///     删除完成之后
    /// </summary>
    /// <param name="req"></param>
    /// <param name="total"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task DeleteAfterAsync(List<Guid> req, int total, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(_cacheKey, cancellationToken);
    }
}