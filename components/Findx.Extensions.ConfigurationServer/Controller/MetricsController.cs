using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions.ConfigurationServer.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Extensions.ConfigurationServer.Controller;

/// <summary>
/// 配置服务-度量信息
/// </summary>
[Area("findx")]
[Route("api/config/metrics")]
[Authorize]
[ApiExplorerSettings(GroupName = "config"), Tags("配置服务-度量信息"), Description("配置服务-度量信息")]
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
    [HttpGet("clients"), Description("客户端清单")]
    public CommonResult Clients()
    {
        return CommonResult.Success(_clientCallBack.Metrics());
    }
}