using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
    /// <returns></returns>
    [HttpGet("list")]
    public async Task<CommonResult> Discovery([FromServices] IDiscoveryClient client, [Required] string serviceName)
    {
        var all = await client.GetAllEndPointsAsync();
        var list = await client.GetServiceEndPointsAsync(serviceName);
        return CommonResult.Success(new { AllInstances = all, Instances = list, client.ProviderName });
    }

    /// <summary>
    ///     服务发现+负载查询示例接口
    /// </summary>
    /// <param name="client"></param>
    /// <param name="loadBalancer"></param>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    [HttpGet("balancer")]
    public async Task<CommonResult> LoadBalancer([FromServices] IDiscoveryClient client, [FromServices] ILoadBalancerProvider loadBalancer, [Required] string serviceName, string loadBalancerType = "Random")
    {
        var balancer = await loadBalancer.GetAsync(serviceName, loadBalancerType.To<LoadBalancerType>());
        var instance = await balancer.ResolveServiceEndPointAsync();

        return CommonResult.Success(new { Instance = instance, LoadBalancer = balancer.Name.ToString() });
    }
}