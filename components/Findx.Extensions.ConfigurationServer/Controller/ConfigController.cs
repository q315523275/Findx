using System.ComponentModel;
using System.Net;
using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc;
using Findx.Common;
using Findx.Data;
using Findx.Extensions.ConfigurationServer.Client;
using Findx.Extensions.ConfigurationServer.Dtos;
using Findx.Extensions.ConfigurationServer.Models;
using Findx.Serialization;
using Findx.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Extensions.ConfigurationServer.Controller;

/// <summary>
///     客户端调用服务
/// </summary>
[Area("findx")]
[Route("api/config")]
[ApiExplorerSettings(GroupName = "config"), Tags("配置服务-客户端服务"), Description("配置服务-客户端服务")]
public class ConfigController : AreaApiControllerBase
{
    private readonly IRepository<AppInfo, long> _appRepo;
    private readonly IRepository<ConfigDataInfo, long> _configRepo;
    private readonly IClientCallBack _clientCallBack;
    private readonly IObjectSerializer _serializer;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="clientCallBack"></param>
    /// <param name="appRepo"></param>
    /// <param name="configRepo"></param>
    /// <param name="serializer"></param>
    public ConfigController(IClientCallBack clientCallBack, IRepository<AppInfo, long> appRepo, IRepository<ConfigDataInfo, long> configRepo, IObjectSerializer serializer)
    {
        _clientCallBack = clientCallBack;
        _appRepo = appRepo;
        _configRepo = configRepo;
        _serializer = serializer;
    }

    /// <summary>
    ///     获取配置
    /// </summary>
    /// <param name="appId">appId</param>
    /// <param name="sign">签名</param>
    /// <param name="environment">环境变量</param>
    /// <param name="version">版本号</param>
    /// <param name="load"></param>
    [HttpGet, Description("获取配置")]
    public async Task GetAsync([Required] string appId, [Required] string sign, [Required] string environment, [Required] long version = 0, bool load = false)
    {
        var model = await _appRepo.FirstAsync(x => x.AppId == appId);
        Check.NotNull(model, nameof(model));
        
        // 验证签名
        var verifySign = EncryptUtility.Md5By32($"{appId}{model.Secret}{environment}{version}");
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
            // 注册变更监听 TaskCompletionSource
            var res = await _clientCallBack.NewCallBackTaskAsync($"{appId}-{environment}", HttpContext.TraceIdentifier, HttpContext.GetClientIp(), 30);
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
}