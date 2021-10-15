using Findx.WebHost.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace Findx.WebHost.Controllers
{
    public class WebApiController: Controller
    {
        /// <summary>
        /// WebApiClient声明式Http请求
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        [HttpGet("/webApi/discovery")]
        public async Task<string> WebApiClientDiscovery([FromServices] IFindxApi api)
        {
            return await api.ApplicationInfo();
        }

        /// <summary>
        /// WebApiClient声明式Http请求
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        [HttpGet("/webApi/discovery/Exception")]
        public async Task<string> WebApiClientDiscoveryException([FromServices] IFindxApi api)
        {
            return await api.Exception();
        }

        /// <summary>
        /// WebApiClient声明式Http请求
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        [HttpGet("/webApi/discovery/timeout")]
        public async Task<string> WebApiClientDiscoveryTimeout([FromServices] IFindxApi api)
        {
            return await api.Timeout();
        }

        /// <summary>
        /// Policy策略Http请求
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        [HttpGet("/policy/httpRequest")]
        public async Task<string> HttpClientPolicy([FromServices] IHttpClientFactory api)
        {
            var client = api.CreateClient("policy");
            return await client.GetStringAsync("http://127.0.0.1:8888/exception");
        }
    }
}
