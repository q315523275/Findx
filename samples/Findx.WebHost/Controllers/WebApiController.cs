using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.WebHost.WebApiClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
///     声明式WebApi服务
/// </summary>
[Route("api/webApi")]
[Description("声明式WebApi服务"), Tags("声明式WebApi服务")]
public class WebApiController : ApiControllerBase
{
    /// <summary>
    ///     WebApiClient声明式Http请求
    /// </summary>
    /// <param name="api"></param>
    /// <returns></returns>
    [HttpGet("discovery")]
    public async Task<string> WebApiClientDiscovery([FromServices] IFindxApi api)
    {
        return await api.ApplicationInfo();
    }

    /// <summary>
    ///     WebApiClient声明式Http请求
    /// </summary>
    /// <param name="api"></param>
    /// <returns></returns>
    [HttpGet("discovery/Exception")]
    public async Task<string> WebApiClientDiscoveryException([FromServices] IFindxApi api)
    {
        return await api.Exception();
    }

    /// <summary>
    ///     WebApiClient声明式Http请求
    /// </summary>
    /// <param name="api"></param>
    /// <returns></returns>
    [HttpGet("discovery/timeout")]
    public async Task<string> WebApiClientDiscoveryTimeout([FromServices] IFindxApi api)
    {
        return await api.Timeout();
    }

    /// <summary>
    ///     Policy策略Http请求
    /// </summary>
    /// <param name="api"></param>
    /// <returns></returns>
    [HttpGet("policy/httpRequest")]
    public async Task<string> HttpClientPolicy([FromServices] IHttpClientFactory api)
    {
        var client = api.CreateClient("policy");
        return await client.GetStringAsync("http://127.0.0.1:8888/exception");
    }
}