using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Data;
using Findx.Extensions.ConfigurationServer.Dtos;
using Findx.Extensions.ConfigurationServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.ConfigurationServer.Controller;

/// <summary>
///     配置服务 - 集群
/// </summary>
[Area("findx")]
[Route("api/config/cluster")]
[ApiExplorerSettings(GroupName = "config"), Tags("配置服务-集群"), Description("配置服务集群")]
public class ClusterController : AreaApiControllerBase
{
    private readonly IDumpService _dumpService;
    private readonly IOptions<ConfigServerOptions> _options;

    /// <summary>
    ///     Ctor
    /// </summary>
    public ClusterController(IDumpService dumpService, IOptions<ConfigServerOptions> options)
    {
        _dumpService = dumpService;
        _options = options;
    }

    /// <summary>
    ///     变更通知接收
    /// </summary>
    [HttpPost("change-notify"), Description("变更通知接收")]
    [InternalNetworkLimiter]
    public async Task<CommonResult> ChangeNotifyAsync([FromBody] ConfigDataChangeDto req, CancellationToken cancellationToken)
    {
        //  集群节点互信校验：配置了 ClusterToken 时必须携带一致令牌（防内网伪造注入配置变更）
        var clusterToken = _options.Value.ClusterToken;
        if (clusterToken.IsNotNullOrWhiteSpace())
        {
            var headerToken = HttpContext.Request.Headers["X-Cluster-Token"].FirstOrDefault();
            if (!string.Equals(headerToken, clusterToken, StringComparison.Ordinal))
                return CommonResult.Fail("auth.fail", "集群令牌无效");
        }

        await _dumpService.DumpAsync(req, cancellationToken);
        return CommonResult.Success();
    }
}
