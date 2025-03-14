using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Expressions;
using Findx.Extensions.WorkflowCore.Dtos.Model;
using Findx.Extensions.WorkflowCore.Entity;
using Findx.Mapping;
using Findx.Module.EleAdminPlus.Shared.ServiceDefaults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Extensions.WorkflowCore.Controllers;

/// <summary>
///     工作流-模型管理
/// </summary>
[Area("flw")]
[Route("api/[area]/model")]
[Authorize]
[ApiExplorerSettings(GroupName = "Workflow"), Tags("工作流-模型管理"), Description("工作流-模型管理")]
public class ModelController: AreaApiControllerBase
{
    private readonly IWorkContext _workContext;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="workContext"></param>
    public ModelController(IWorkContext workContext)
    {
        _workContext = workContext;
    }

    /// <summary>
    ///     构建分页查询条件
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    private Expression<Func<FlwProcessInfo, bool>> CreateWhereExpression(ModelQueryDto req)
    {
        var whereExp = PredicateBuilder.New<FlwProcessInfo>()
                                       .AndIf(req.Name.IsNotNullOrWhiteSpace(), x => x.Name.Contains(req.Name));
        return whereExp.Build();
    }

    /// <summary>
    ///     构建分页查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private IEnumerable<SortCondition<FlwProcessInfo>> CreateOrderExpression(ModelQueryDto request)
    {
        var orderExp = SortConditionBuilder.New<FlwProcessInfo>();
        if (request is SortCondition sortCondition && sortCondition.SortField.IsNotNullOrWhiteSpace())
            orderExp.Order(request.SortField, request.SortDirection);
        return orderExp.OrderBy(it => it.Id).Build();
    }
    
    /// <summary>
    ///     分页查询
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("page")]
    public async Task<CommonResult> PageAsync([FromQuery] ModelPageQueryDto req, CancellationToken cancellationToken)
    {
        var repo = GetRepository<FlwProcessInfo, long>();

        var whereExpression = CreateWhereExpression(req);
        var orderByExpression = CreateOrderExpression(req);
        
        var rs = await repo.PagedAsync<ModelSimplifyDto>(req.PageNo, req.PageSize, whereExpression, sortConditions: orderByExpression, cancellationToken: cancellationToken);

        return CommonResult.Success(rs);
    }

    /// <summary>
    ///     新增模型
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("add")]
    public async Task<CommonResult> AddAsync([FromBody] ModelAddDto req, CancellationToken cancellationToken)
    {
        var repo = GetRepository<FlwProcessInfo, long>();
        
        var model = req.MapTo<FlwProcessInfo>();
        model.CheckCreationAudited<FlwProcessInfo, long>(HttpContext.User);
        model.CheckOrg<FlwProcessInfo, long>(HttpContext.User);
        model.CheckOwner<FlwProcessInfo, long>(HttpContext.User);
        model.CheckTenant<FlwProcessInfo, Guid>(HttpContext.User);
        
        await repo.InsertAsync(model, cancellationToken);
        
        return CommonResult.Success();
    }
    
    /// <summary>
    ///     编辑模型
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("edit")]
    public async Task<CommonResult> EditAsync([FromBody] ModelEditDto req, CancellationToken cancellationToken)
    {
        var repo = GetRepository<FlwProcessInfo, long>();
        
        var model = req.MapTo<FlwProcessInfo>();
        repo.Attach(model.Clone());
        model.CheckUpdateAudited<FlwProcessInfo, long>(HttpContext.User);
        
        await repo.SaveAsync(model, cancellationToken);
        
        return CommonResult.Success();
    }
}