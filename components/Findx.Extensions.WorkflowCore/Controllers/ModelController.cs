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
    private readonly FlwBusinessAdapterRegistry _adapters;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="workContext"></param>
    /// <param name="adapters">业务模块注册表，可为空（宿主未注册任何 IFlwBusinessAdapter）</param>
    public ModelController(IWorkContext workContext, FlwBusinessAdapterRegistry adapters = null)
    {
        _workContext = workContext;
        _adapters = adapters;
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
    ///     已注册的业务模块列表
    ///     <para>审批管理「基础信息」选择业务模块时使用；集合来自宿主注册的全部 IFlwBusinessAdapter 实现</para>
    /// </summary>
    /// <returns></returns>
    [HttpGet("business-types")]
    public CommonResult BusinessTypes()
    {
        var adapters = _adapters?.Adapters ?? Array.Empty<IFlwBusinessAdapter>();
        var rows = adapters
                   .Where(x => x != null && !string.IsNullOrWhiteSpace(x.BusinessType))
                   .Select(x => new BusinessTypeVo
                   {
                       BusinessType = x.BusinessType,
                       DisplayName = string.IsNullOrWhiteSpace(x.DisplayName) ? x.BusinessType : x.DisplayName
                   })
                   .OrderBy(x => x.DisplayName)
                   .ToList();
        return CommonResult.Success(rows);
    }

    /// <summary>
    ///     业务单据可用的条件字段（流程设计器条件分支下拉用）
    /// </summary>
    /// <param name="businessType">业务类型标识，如 pms.order</param>
    [HttpGet("business-fields")]
    public CommonResult BusinessFields([FromQuery] string businessType)
    {
        var adapter = _adapters?.Get(businessType);
        var rows = adapter?.GetFields() ?? Array.Empty<FlwBusinessField>();
        return CommonResult.Success(rows);
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
        var rows = paged.Rows.Select(x =>
        {
            var dto = x.MapTo<ModelSimplifyDto>();
            //  列表只需要知道有没有配表单，不必把整段 FormSchema 传下去
            dto.HasForm = !string.IsNullOrWhiteSpace(x.FormSchema);
            return dto;
        }).ToList();
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

        var existing = await repo.GetAsync(req.Id, cancellationToken);
        if (existing == null) return CommonResult.Fail("404", "流程模型不存在");
        //  历史版本只读：允许改动就失去"历史"意义，回溯也就无从谈起（要改请复制后修改）
        if (existing.Status == 2) return CommonResult.Fail("flow.history.readonly", "历史版本只读，请复制该版本后再修改");

        var model = req.MapTo<FlwProcessInfo>();
        model.CheckUpdateAudited<FlwProcessInfo, long>(HttpContext.User);

        //  版本口径：每次"已发布流程的内容"发生变化就升一版，旧内容冻结为历史版本。
        //  在途实例各自持有启动时的模型快照（flw_ext_instance.ModelContent），
        //  所以改定义只会影响新发起的单据，不会让已在跑的流程断掉。
        var version = existing.Version <= 0 ? 1 : existing.Version;
        if (existing.Status == 1 && !string.Equals(existing.ModelContent, model.ModelContent, StringComparison.Ordinal))
        {
            await SnapshotHistoryAsync(repo, existing, cancellationToken);
            version++;
        }

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
            GroupName = model.GroupName,
            Comments = model.Comments,
            Version = version,
            LastUpdater = model.LastUpdater,
            LastUpdaterId = model.LastUpdaterId,
            LastUpdatedTime = model.LastUpdatedTime
        }, x => x.Id == model.Id, cancellationToken);

        return CommonResult.Success(new { version });
    }

    /// <summary>
    ///     把流程当前内容冻结成一条历史版本记录
    ///     <para>复制整行只换主键：名称/编码/分组/表单/租户与创建信息全部保留，回溯时可直接重新发布该行</para>
    /// </summary>
    /// <param name="repo">流程定义仓储</param>
    /// <param name="source">要被覆盖的已发布记录</param>
    /// <param name="cancellationToken">取消标记</param>
    private static async Task SnapshotHistoryAsync(IRepository<FlwProcessInfo, long> repo, FlwProcessInfo source, CancellationToken cancellationToken)
    {
        var history = source.Clone();
        history.Id = 0;
        history.SetEmptyKey();
        history.Status = 2;
        history.Version = source.Version <= 0 ? 1 : source.Version;
        await repo.InsertAsync(history, cancellationToken);
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

        //  同一业务类型只允许一条生效流程：否则 FindProcessAsync 只能按版本/Id 兜底挑一条，
        //  等于悄悄抢占，发起时会用错流程。要切换请先停用旧流程。
        if (!string.IsNullOrWhiteSpace(model.BusinessType))
        {
            var conflicts = await repo.SelectAsync(
                x => x.BusinessType == model.BusinessType && x.Code != model.Code && x.Status == 1,
                cancellationToken: cancellationToken);
            if (conflicts is { Count: > 0 })
            {
                return CommonResult.Fail("flow.business.occupied",
                    $"业务模块[{model.BusinessType}]已有生效流程：{string.Join("、", conflicts.Select(x => x.Name))}，请先停用后再发布");
            }
        }

        foreach (var item in sameCodeModels.Where(x => x.Status == 1))
        {
            item.Status = 2;
            item.CheckUpdateAudited<FlwProcessInfo, long>(HttpContext.User);
            await repo.UpdateAsync(item, cancellationToken: cancellationToken);
        }

        //  版本号按同编码最大版本 +1 递增：复制出来的新版本、回溯发布的历史版本都不会与旧版本重号
        var maxOtherVersion = sameCodeModels.Select(x => x.Version).DefaultIfEmpty(0).Max();
        var version = model.Version <= 0 ? 0 : model.Version;
        if (version <= maxOtherVersion) version = maxOtherVersion + 1;

        model.Version = version;
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

    /// <summary>
    ///     停用流程：仅影响新发起，在途实例按各自快照继续走完；停用后可重新发布或删除
    /// </summary>
    /// <param name="id">流程模型 Id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("disable")]
    public async Task<CommonResult> DisableAsync([FromQuery] long id, CancellationToken cancellationToken)
    {
        var repo = GetRepository<FlwProcessInfo, long>();

        var model = await repo.GetAsync(id, cancellationToken);
        if (model == null) return CommonResult.Fail("404", "流程模型不存在");
        if (model.Status == 2) return CommonResult.Fail("flow.history.readonly", "历史版本无需停用，直接发布目标版本即可");
        if (model.Status != 1) return CommonResult.Fail("flow.not.published", $"流程[{model.Name}]当前不是生效状态");

        model.CheckUpdateAudited<FlwProcessInfo, long>(HttpContext.User);
        await repo.UpdateColumnsAsync(x => new FlwProcessInfo
        {
            Status = 0,
            LastUpdater = model.LastUpdater,
            LastUpdaterId = model.LastUpdaterId,
            LastUpdatedTime = model.LastUpdatedTime
        }, x => x.Id == model.Id, cancellationToken);

        return CommonResult.Success();
    }

    /// <summary>
    ///     删除模型
    ///     <para>已发布（Status=1）的流程不允许删除，避免在途实例失去流程定义；需先停用</para>
    /// </summary>
    /// <param name="req">要删除的模型 Id 集合</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("delete")]
    public async Task<CommonResult> DeleteAsync([FromBody] List<long> req, CancellationToken cancellationToken)
    {
        if (req is not { Count: > 0 }) return CommonResult.Fail("400", "请选择要删除的流程");

        var repo = GetRepository<FlwProcessInfo, long>();
        var models = await repo.SelectAsync(x => req.Contains(x.Id), cancellationToken: cancellationToken);
        if (models.Count == 0) return CommonResult.Fail("404", "流程模型不存在");

        var published = models.Where(x => x.Status == 1).Select(x => x.Name).ToList();
        if (published.Count > 0)
            return CommonResult.Fail("flow.published", $"已发布的流程不能删除，请先停用：{string.Join("、", published)}");

        await repo.DeleteAsync(x => req.Contains(x.Id), cancellationToken);
        return CommonResult.Success();
    }
}