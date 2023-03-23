using System.ComponentModel;
using System.Security.Principal;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Linq;
using Findx.Mapping;
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
    private readonly IApplicationContext _applicationContext;
    private readonly IRepository<ConfigInfo> _configRepo;
    private readonly IRepository<ConfigHistoryInfo> _configHistoryRepo;
    private readonly IPrincipal _principal;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="applicationContext"></param>
    /// <param name="configRepo"></param>
    /// <param name="principal"></param>
    public ConfigController(IApplicationContext applicationContext, IRepository<ConfigInfo> configRepo, IPrincipal principal, IRepository<ConfigHistoryInfo> configHistoryRepo)
    {
        _applicationContext = applicationContext;
        _configRepo = configRepo;
        _principal = principal;
        _configHistoryRepo = configHistoryRepo;
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
            await _configRepo.InsertAsync(model);
            // 发布ConfigDataChangeEvent事件
            await _applicationContext.PublishEventAsync(model.MapTo<ConfigDataChangeEvent>());
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
            await _configRepo.UpdateAsync(dbConfig, ignoreColumns: x => new { x.Environment, x.AppId, x.CreatedTime, x.CreatorId });
            await _configHistoryRepo.InsertAsync(hisConfig);
            // 发布ConfigDataChangeEvent事件
            await _applicationContext.PublishEventAsync(dbConfig.MapTo<ConfigDataChangeEvent>());
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