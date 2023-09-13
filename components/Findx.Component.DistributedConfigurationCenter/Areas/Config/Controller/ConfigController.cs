using System.ComponentModel;
using System.Security.Principal;
using Findx.AspNetCore.Mvc;
using Findx.Component.DistributedConfigurationCenter.Dtos;
using Findx.Component.DistributedConfigurationCenter.Handling;
using Findx.Component.DistributedConfigurationCenter.Models;
using Findx.Data;
using Findx.Events;
using Findx.Extensions;
using Findx.Linq;
using Findx.Mapping;
using Findx.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Component.DistributedConfigurationCenter.Areas.Config.Controller;

/// <summary>
///     配置服务-管理
/// </summary>
[Area("findx")]
[Route("api/config/manage")]
[Authorize]
[Description("配置服务-管理")]
[ApiExplorerSettings(GroupName = "config"), Tags("配置服务-管理")]
public class ConfigController : AreaApiControllerBase
{
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IRepository<ConfigHistoryInfo> _historyRepo;
    private readonly IRepository<ConfigInfo> _configRepo;
    private readonly IEventBus _eventBus;
    private readonly IPrincipal _principal;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="unitOfWorkManager"></param>
    /// <param name="historyRepo"></param>
    /// <param name="configRepo"></param>
    /// <param name="eventBus"></param>
    /// <param name="principal"></param>
    public ConfigController(IUnitOfWorkManager unitOfWorkManager, IRepository<ConfigHistoryInfo> historyRepo, IRepository<ConfigInfo> configRepo, IEventBus eventBus, IPrincipal principal)
    {
        _unitOfWorkManager = unitOfWorkManager;
        _historyRepo = historyRepo;
        _configRepo = configRepo;
        _eventBus = eventBus;
        _principal = principal;
    }

    /// <summary>
    ///     分页查询
    /// </summary>
    /// <param name="req">查询条件信息</param>
    /// <returns></returns>
    [HttpGet("page")]
    public async Task<CommonResult> PageAsync([FromQuery] QueryConfigDto req)
    {
        var whereExp = PredicateBuilder.New<ConfigInfo>()
                                       .AndIf(!req.DataId.IsNullOrWhiteSpace(), x => x.DataId.Contains(req.DataId))
                                       .AndIf(!req.AppId.IsNullOrWhiteSpace(), x => x.AppId == req.AppId)
                                       .AndIf(!req.Environment.IsNullOrWhiteSpace(), x => x.Environment == req.Environment)
                                       .Build();

        var rows = await _configRepo.PagedAsync<ConfigSimpleDto>(req.PageNo, req.PageSize, whereExp);

        return CommonResult.Success(rows);
    }

    /// <summary>
    ///     获取配置信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("getConfig")]
    public async Task<CommonResult> GetConfigAsync(Guid id)
    {
        var model = await _configRepo.GetAsync(id);
        return CommonResult.Success(model);
    }

    /// <summary>
    ///     删除配置信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("delete")]
    public async Task<CommonResult> DeleteAsync(Guid id)
    {
        await _configRepo.DeleteAsync(id);
        return CommonResult.Success();
    }

    /// <summary>
    ///     历史配置列表
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("history")]
    public async Task<CommonResult> HistoryAsync(Guid id)
    {
        var model = await _configRepo.GetAsync(id);
        if (model != null)
            return CommonResult.Fail("no.found", "源配置信息不存在");
        
        var whereExp = PredicateBuilder.New<ConfigHistoryInfo>()
                                       .And(x => x.AppId == model.AppId)
                                       .And(x => x.Environment == model.Environment)
                                       .And(x => x.DataId == model.DataId)
                                       .Build();
        var orderExp = DataSortBuilder.New<ConfigHistoryInfo>().OrderByDescending(x => x.Version).Build();
        
        var rows = await _historyRepo.SelectAsync(whereExpression: whereExp, orderParameters: orderExp);
        
        return CommonResult.Success(rows);
    }

    /// <summary>
    ///     发布配置
    /// </summary>
    /// <param name="req">配置信息</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("publishConfig")]
    public async Task<CommonResult> PublishConfigAsync([FromBody] PublishConfigDto req, CancellationToken cancellationToken)
    {
        var dbConfig = await _configRepo.FirstAsync(x => x.DataId == req.DataId, cancellationToken);
        if (dbConfig == null)
        {
            // 新配置
            
            var model = req.MapTo<ConfigInfo>();
            model.SetEmptyKey();
            model.CheckCreationAudited<ConfigInfo, Guid>(_principal);
            model.Version = DateTime.Now.ToString("yyyyMMddHHmmssfff").To<long>();
            model.Md5 = EncryptUtility.Md5By32(req.Content);
            // 保存并通知集群
            await using var uow = await _unitOfWorkManager.GetEntityUnitOfWorkAsync<ConfigInfo>(true, true, cancellationToken);
            await _configRepo.WithUnitOfWork(uow).InsertAsync(model, cancellationToken);
            // 发布ConfigDataChangeEvent事件,等待执行
            await _eventBus.PublishAsync(model.MapTo<ConfigDataChangeEvent>(), cancellationToken);
            // 提交事物
            await uow.CommitAsync(cancellationToken);
        }
        else if (dbConfig.Md5 != EncryptUtility.Md5By32(req.Content))
        {
            // 修改配置
            
            // 历史记录
            var hisConfig = dbConfig.MapTo<ConfigHistoryInfo>();
            hisConfig.Id = Guid.Empty;
            hisConfig.SetEmptyKey();
            hisConfig.CheckCreationAudited<ConfigHistoryInfo, Guid>(_principal);
            // 修改内容
            dbConfig.CheckUpdateAudited<ConfigInfo, Guid>(_principal);
            dbConfig.Comment = req.Comment;
            dbConfig.DataType = req.DataType;
            dbConfig.Content = req.Content;
            dbConfig.Version = DateTime.Now.ToString("yyyyMMddHHmmssfff").To<long>();
            dbConfig.Md5 = EncryptUtility.Md5By32(req.Content);
            // 保存并通知集群
            await using var uow = await _unitOfWorkManager.GetEntityUnitOfWorkAsync<ConfigInfo>(true, true, cancellationToken);
            await _configRepo.WithUnitOfWork(uow).UpdateAsync(dbConfig, ignoreColumns: x => new { x.Environment, x.AppId, x.CreatedTime, x.CreatorId }, cancellationToken: cancellationToken);
            await _historyRepo.WithUnitOfWork(uow).InsertAsync(hisConfig, cancellationToken);
            // 发布ConfigDataChangeEvent事件,等待执行
            await _eventBus.PublishAsync(dbConfig.MapTo<ConfigDataChangeEvent>(), cancellationToken);
            // 提交事物
            await uow.CommitAsync(cancellationToken);
        }
        else
        {
            // 修改配置,但是配置内容未修改
            
            dbConfig.CheckUpdateAudited<ConfigInfo, Guid>(_principal);
            dbConfig.Comment = req.Comment;
            dbConfig.DataType = req.DataType;
            await _configRepo.UpdateAsync(dbConfig, x => new { x.Comment, x.DataType, x.CreatedTime, x.CreatorId }, cancellationToken: cancellationToken);
        }

        return CommonResult.Success();
    }
}