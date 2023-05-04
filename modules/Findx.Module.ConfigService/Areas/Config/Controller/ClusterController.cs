using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Data;
using Findx.Module.ConfigService.Dtos;
using Findx.Module.ConfigService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.ConfigService.Areas.Config.Controller;

/// <summary>
///     配置服务 - 集群
/// </summary>
[Area("findx")]
[Route("api/config/cluster")]
[Description("配置服务集群")]
[ApiExplorerSettings(GroupName = "config")]
[Tags("配置服务-集群")]
public class ClusterController : AreaApiControllerBase
{
    private readonly IDumpService _dumpService;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="dumpService"></param>
    public ClusterController(IDumpService dumpService)
    {
        _dumpService = dumpService;
    }

    /// <summary>
    ///     集群配置变更通知接收
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("configChangeNotify")]
    [InternalNetworkLimiter]
    public async Task<CommonResult> ClusterConfigChangeNotifyAsync([FromBody] ConfigDataChangeDto req,
        CancellationToken cancellationToken)
    {
        await _dumpService.DumpAsync(req, cancellationToken);
        return CommonResult.Success();
    }
}