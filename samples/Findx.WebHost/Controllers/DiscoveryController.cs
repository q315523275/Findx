using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Discovery;
using Findx.Discovery.Abstractions;
using Findx.Discovery.LoadBalancer;
using Findx.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
///     服务发现
/// </summary>
[Route("api/discovery")]
[Description("服务发现"), Tags("服务发现")]
public class DiscoveryController : ApiControllerBase
{
    /// <summary>
    ///     服务发现查询示例接口
    /// </summary>
    /// <param name="client"></param>
    /// <param name="serviceName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("list")]
    public async Task<CommonResult> DiscoveryAsync([FromServices] IDiscoveryClient client, [Required] string serviceName, CancellationToken cancellationToken)
    {
        var all = await client.GetAllEndPointsAsync(cancellationToken: cancellationToken);
        var list = await client.GetServiceEndPointsAsync(serviceName, cancellationToken: cancellationToken);
        return CommonResult.Success(new { AllInstances = all, Instances = list, client.ProviderName });
    }

    /// <summary>
    ///     服务发现+负载查询示例接口
    /// </summary>
    /// <param name="loadBalancer"></param>
    /// <param name="serviceName"></param>
    /// <param name="loadBalancerType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("balancer")]
    public async Task<CommonResult> LoadBalancerAsync([FromServices] ILoadBalancerProvider loadBalancer, [Required] string serviceName, string loadBalancerType = "Random", CancellationToken cancellationToken = default)
    {
        var balancer = await loadBalancer.GetAsync(serviceName, loadBalancerType.To<LoadBalancerType>());
        var instance = await balancer.ResolveServiceEndPointAsync(cancellationToken);

        return CommonResult.Success(new { Instance = instance, LoadBalancer = balancer.Name.ToString() });
    }
}