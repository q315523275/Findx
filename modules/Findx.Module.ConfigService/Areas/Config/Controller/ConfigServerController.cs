using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Module.ConfigService.Client;
using Findx.Module.ConfigService.Dtos;
using Findx.Module.ConfigService.Models;
using Findx.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.ConfigService.Areas.Config.Controller;

[Area("findx")]
[Route("api/config")]
[Description("配置服务-获取")]
public class ConfigServerController: AreaApiControllerBase
{
    private readonly IClientCallBack _clientCallBack;
    private readonly IRepository<AppInfo> _appRepo;
    private readonly IRepository<ConfigInfo> _configRepo;
    private readonly ISerializer _serializer;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="clientCallBack"></param>
    /// <param name="appRepo"></param>
    /// <param name="configRepo"></param>
    /// <param name="serializer"></param>
    public ConfigServerController(IClientCallBack clientCallBack, IRepository<AppInfo> appRepo, IRepository<ConfigInfo> configRepo, ISerializer serializer)
    {
        _clientCallBack = clientCallBack;
        _appRepo = appRepo;
        _configRepo = configRepo;
        _serializer = serializer;
    }

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <param name="appId">appId</param>
    /// <param name="reqId">客户端编号</param>
    /// <param name="sign">签名</param>
    /// <param name="environment">环境变量</param>
    /// <param name="version">版本号</param>
    /// <param name="load"></param>
    [HttpGet]
    public async Task GetAsync([Required] string appId, [Required] string sign, [Required] string environment, [Required] string reqId, [Required] long version = 0, bool load = false)
    {
        var model = await _appRepo.FirstAsync(x => x.AppId == appId);
        Check.NotNull(model, nameof(model));
        // 验证签名
        var verifySign = Utils.Encrypt.Md5By32($"{appId}{model.Secret}{reqId}{environment}{version}");
        if (verifySign != sign)
        {
            Response.StatusCode = HttpStatusCode.Forbidden.To<int>();
            return;
        }
        // 是否初次加载或指定加载
        if (version == 0 || load)
        {
            var rows = await _configRepo.SelectAsync(x => x.AppId == appId && x.Environment == environment && x.Version > version, x => new { x.DataId, x.DataType, x.Content, x.Version });
            // 返回监听结果
            Response.ContentType = "application/json; charset=utf-8";
            await Response.Body.WriteAsync(_serializer.Serialize(rows));
            return;
        }
        // 变更监听
        try
        {
            // 注册变更监听TaskCompletionSource
            var res = await _clientCallBack.NewCallBackTaskAsync($"{appId}-{environment}", reqId, HttpContext.GetClientIp(), 20);
            var rows = new List<ConfigDataChangeDto> { res };
            // 返回监听结果
            Response.ContentType = "application/json; charset=utf-8";
            await Response.Body.WriteAsync(_serializer.Serialize(rows));
        }
        catch (TimeoutException)
        {
            Response.StatusCode = HttpStatusCode.NoContent.To<int>();
        }
    }
    
    /// <summary>
    /// 设置数据(演示)
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="environment"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPut]
    public CommonResult PutAsync(string appId, string environment, [FromBody] ConfigDataChangeDto value)
    {
       _clientCallBack.CallBack($"{appId}-{environment}", value);

       return CommonResult.Success();
    }
}