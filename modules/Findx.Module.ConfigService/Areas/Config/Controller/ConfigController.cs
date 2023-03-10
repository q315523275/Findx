using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Linq;
using Findx.Mapping;
using Findx.Module.ConfigService.Dtos;
using Findx.Module.ConfigService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.ConfigService.Areas.Config.Controller;

[Area("findx")]
[Route("api/[area]/config")]
[Authorize]
[Description("框架-配置")]
public class ConfigController: AreaApiControllerBase
{
    private readonly IApplicationContext _applicationContext;
    private readonly IRepository<ConfigInfo> _configRepo;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="applicationContext"></param>
    /// <param name="configRepo"></param>
    public ConfigController(IApplicationContext applicationContext, IRepository<ConfigInfo> configRepo)
    {
        _applicationContext = applicationContext;
        _configRepo = configRepo;
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
                                        .AndIF(!req.Group.IsNullOrWhiteSpace(), x => x.GroupId.Contains(req.Group))
                                        .AndIF(!req.AppName.IsNullOrWhiteSpace(), x => x.AppName.Contains(req.AppName))
                                        .AndIF(!req.Environment.IsNullOrWhiteSpace(), x => x.Environment.Contains(req.Environment))
                                        .ToExpression();

        var rows = await _configRepo.PagedAsync(req.PageNo, req.PageSize, whereExp);
        
        return CommonResult.Success(rows);
    }


    /// <summary>
    /// 发布配置
    /// </summary>
    /// <param name="req">配置信息</param>
    /// <returns></returns>
    [HttpPost("publishConfig")]
    public async Task<CommonResult> PublishConfigAsync([FromBody] PublishConfigDto req)
    {
        var model = req.MapTo<ConfigInfo>();
        
        
        
        return CommonResult.Success();
    }

}