using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.WebHost.Aspect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
///     动态代理
/// </summary>
[Route("api/dynamicproxy")]
[Tags("动态代理"), Description("动态代理")]
public class AspectController : AreaApiControllerBase
{
    /// <summary>
    ///     接口动态代理
    /// </summary>
    /// <returns></returns>
    [HttpGet("aspect")]
    public CommonResult ApplicationInfo()
    {
        var test = (IMachine)ServiceLocator.GetService(typeof(IMachine));
        test.Purchase(1999999);
        return CommonResult.Success();
    }
}