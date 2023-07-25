using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Component.DistributedConfigurationCenter.Client;
using Findx.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Component.DistributedConfigurationCenter.Areas.Config.Controller;

/// <summary>
/// 配置服务-度量信息
/// </summary>
[Area("findx")]
[Route("api/config/metrics")]
[Authorize]
[Description("配置服务-度量信息")]
[ApiExplorerSettings(GroupName = "config"), Tags("配置服务-度量信息")]
public class MetricsController: AreaApiControllerBase
{
    private readonly IClientCallBack _clientCallBack;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="clientCallBack"></param>
    public MetricsController(IClientCallBack clientCallBack)
    {
        _clientCallBack = clientCallBack;
    }
    
    /// <summary>
    ///     客户端连接信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("clients")]
    public CommonResult Clients()
    {
        return CommonResult.Success(_clientCallBack.Metrics());
    }
}