using System.ComponentModel;
using System.Security.Principal;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Linq;
using Findx.Mapping;
using Findx.Messaging;
using Findx.Module.ConfigService.Dtos;
using Findx.Module.ConfigService.Handling;
using Findx.Module.ConfigService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.ConfigService.Areas.Config.Controller;

[Area("findx")]
[Route("api/config/manage")]
[Authorize]
[Description("配置服务-管理")]
[ApiExplorerSettings(GroupName = "config")]
public class ConfigController: AreaApiControllerBase
{
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IMessageDispatcher _messageDispatcher;
    private readonly IRepository<ConfigInfo> _configRepo;
    private readonly IRepository<ConfigHistoryInfo> _configHistoryRepo;
    private readonly IPrincipal _principal;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="configRepo"></param>
    /// <param name="principal"></param>
    /// <param name="configHistoryRepo"></param>
    /// <param name="messageDispatcher"></param>
    public ConfigController(IRepository<ConfigInfo> configRepo, IPrincipal principal, IRepository<ConfigHistoryInfo> configHistoryRepo, IMessageDispatcher messageDispatcher, IUnitOfWorkManager unitOfWorkManager)
    {
        _configRepo = configRepo;
        _principal = principal;
        _configHistoryRepo = configHistoryRepo;
        _messageDispatcher = messageDispatcher;
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="req">查询条件信息</param>
    /// <returns></returns>
    [HttpGet("page")]
    public async Task<CommonResult> PageAsync([FromQuery] QueryConfigDto req)
    {
        var whereExp = ExpressionBuilder.Create<ConfigInfo>()
                                        .AndIF(!req.DataId.IsNullOrWhiteSpace(), x => x.DataId.Contains(req.DataId))
                                        .AndIF(!req.AppId.IsNullOrWhiteSpace(), x => x.AppId == req.AppId)
                                        .AndIF(!req.Environment.IsNullOrWhiteSpace(), x => x.Environment == req.Environment)
                                        .ToExpression();

        var rows = await _configRepo.PagedAsync<ConfigSimpleDto>(req.PageNo, req.PageSize, whereExp);
        
        return CommonResult.Success(rows);
    }

    /// <summary>
    /// 获取配置信息
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
    /// 获取配置信息
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
    /// 发布配置
    /// </summary>
    /// <param name="req">配置信息</param>
    /// <returns></returns>
    [HttpPost("publishConfig")]
    public async Task<CommonResult> PublishConfigAsync([FromBody] PublishConfigDto req)
    {
        var dbConfig = await _configRepo.FirstAsync(x => x.DataId == req.DataId);
        if (dbConfig == null)
        {
            var model = req.MapTo<ConfigInfo>();
            model.SetEmptyKey();
            model.CheckCreationAudited<ConfigInfo, Guid>(_principal);
            model.Version = DateTime.Now.ToString("yyyyMMddHHmmssfff").To<long>();
            model.Md5 = Utils.Encrypt.Md5By32(req.Content);
            // 保存并通知集群
            using var uow = await _unitOfWorkManager.GetEntityUnitOfWorkAsync<ConfigInfo>(true, true);
            await _configRepo.WithUnitOfWork(uow).InsertAsync(model);
            // 发布ConfigDataChangeEvent事件,等待执行
            await _messageDispatcher.PublishAsync(model.MapTo<ConfigDataChangeEvent>());
            // 提交事物
            await uow.CommitAsync();
        }
        else if (dbConfig.Md5 != Utils.Encrypt.Md5By32(req.Content))
        {
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
            dbConfig.Md5 = Utils.Encrypt.Md5By32(req.Content);
            // 保存并通知集群
            using var uow = await _unitOfWorkManager.GetEntityUnitOfWorkAsync<ConfigInfo>(true, true);
            await _configRepo.WithUnitOfWork(uow).UpdateAsync(dbConfig, ignoreColumns: x => new { x.Environment, x.AppId, x.CreatedTime, x.CreatorId });
            await _configHistoryRepo.WithUnitOfWork(uow).InsertAsync(hisConfig);
            // 发布ConfigDataChangeEvent事件,等待执行
            await _messageDispatcher.PublishAsync(dbConfig.MapTo<ConfigDataChangeEvent>());
            // 提交事物
            await uow.CommitAsync();
        }
        else
        {
            dbConfig.CheckUpdateAudited<ConfigInfo, Guid>(_principal);
            dbConfig.Comment = req.Comment;
            dbConfig.DataType = req.DataType;
            await _configRepo.UpdateAsync(dbConfig, updateColumns: x => new { x.Comment, x.DataType, x.CreatedTime, x.CreatorId });
        }

        return CommonResult.Success();
    }

}