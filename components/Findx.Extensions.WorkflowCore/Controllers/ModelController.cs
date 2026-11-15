using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Expressions;
using Findx.Extensions.WorkflowCore.Dtos.Model;
using Findx.Extensions.WorkflowCore.Engine;
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

        //  先分页实体再映射 DTO：直接 ToListAsync<ModelSimplifyDto> 时 Id 映射不到，列表拿不到主键就无法发布
        var paged = await repo.PagedAsync(req.PageNo, req.PageSize, whereExpression, sortConditions: orderByExpression, cancellationToken: cancellationToken);
        var rows = paged.Rows.Select(x => x.MapTo<ModelSimplifyDto>()).ToList();
        return CommonResult.Success(new PageResult<List<ModelSimplifyDto>>(paged.PageNo, paged.PageSize, paged.TotalRows, rows));
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

        //  InsertAsync 不补主键，必须显式生成雪花 Id，否则落库恒为 0
        model.SetEmptyKey();

        await repo.InsertAsync(model, cancellationToken);

        return CommonResult.Success(model.Id);
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
        model.CheckUpdateAudited<FlwProcessInfo, long>(HttpContext.User);

        //  显式更新列：Attach(Clone) + Save 的变更比对在这种"整对象覆盖"场景下检测不出差异
        await repo.UpdateColumnsAsync(x => new FlwProcessInfo
        {
            Name = model.Name,
            Icon = model.Icon,
            Type = model.Type,
            InstanceUrl = model.InstanceUrl,
            BusinessType = model.BusinessType,
            FormSchema = model.FormSchema,
            ModelContent = model.ModelContent,
            UseScope = model.UseScope,
            Sort = model.Sort,
            Comments = model.Comments,
            LastUpdater = model.LastUpdater,
            LastUpdaterId = model.LastUpdaterId,
            LastUpdatedTime = model.LastUpdatedTime
        }, x => x.Id == model.Id, cancellationToken);

        return CommonResult.Success();
    }

    /// <summary>
    ///     模型详情（含模型 JSON 内容）
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("detail")]
    public async Task<CommonResult> DetailAsync([FromQuery] long id, CancellationToken cancellationToken)
    {
        var repo = GetRepository<FlwProcessInfo, long>();
        var model = await repo.GetAsync(id, cancellationToken);
        if (model == null) return CommonResult.Fail("404", "流程模型不存在");

        return CommonResult.Success(model);
    }

    /// <summary>
    ///     发布模型：校验模型 JSON，同编码的其它可用版本置为历史版本，当前记录升级版本并置为可用
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("publish")]
    public async Task<CommonResult> PublishAsync([FromQuery] long id, CancellationToken cancellationToken)
    {
        var repo = GetRepository<FlwProcessInfo, long>();

        var model = await repo.GetAsync(id, cancellationToken);
        if (model == null) return CommonResult.Fail("404", "流程模型不存在");

        try
        {
            FlwModelParser.Parse(model.ModelContent, model.Code, model.Name, model.Type);
        }
        catch (FlwModelException ex)
        {
            return CommonResult.Fail("500", ex.Message);
        }

        var sameCodeModels = await repo.SelectAsync(x => x.Code == model.Code && x.Id != model.Id, cancellationToken: cancellationToken);
        foreach (var item in sameCodeModels.Where(x => x.Status == 1))
        {
            item.Status = 2;
            item.CheckUpdateAudited<FlwProcessInfo, long>(HttpContext.User);
            await repo.UpdateAsync(item, cancellationToken: cancellationToken);
        }

        model.Version = model.Version <= 0 ? 1 : model.Version;
        model.Status = 1;
        model.CheckUpdateAudited<FlwProcessInfo, long>(HttpContext.User);
        //  Attach + Save 走的是"比对变更列"路径，这里 Attach 的是已改过的副本，比对不出差异，状态不会落库
        await repo.UpdateColumnsAsync(x => new FlwProcessInfo
        {
            Status = 1,
            Version = model.Version,
            LastUpdater = model.LastUpdater,
            LastUpdaterId = model.LastUpdaterId,
            LastUpdatedTime = model.LastUpdatedTime
        }, x => x.Id == model.Id, cancellationToken);

        return CommonResult.Success();
    }
}