using Findx.Data;
using Findx.Discovery;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Findx.WebHost.Controllers
{
    public class DiscoveryController : Controller
    {
        /// <summary>
        /// 服务发现查询示例接口
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        [HttpGet("/discovery/list")]
        public async Task<CommonResult> Discovery([FromServices] IDiscoveryClient client, [Required] string serviceName)
        {
            var all = await client.GetAllInstancesAsync();
            var list = await client.GetInstancesAsync(serviceName);
            return CommonResult.Success(new { AllInstances = all, Instances = list });
        }

        /// <summary>
        /// 服务发现+负载查询示例接口
        /// </summary>
        /// <param name="client"></param>
        /// <param name="loadBalancer"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        [HttpGet("/discovery/balancer")]
        public async Task<CommonResult> LoadBalancer([FromServices] IDiscoveryClient client, [FromServices] ILoadBalancerProvider loadBalancer, [Required] string serviceName, string loadBalancerType = "Random")
        {
            var balancer = await loadBalancer.GetAsync(serviceName, loadBalancerType.To<LoadBalancerType>());
            var instance = await balancer.ResolveServiceInstanceAsync();

            return CommonResult.Success(new { Instance = instance, LoadBalancer = balancer.Name.ToString() });
        }
    }
}
