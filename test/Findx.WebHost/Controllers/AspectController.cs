using System.ComponentModel;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.WebHost.Aspect;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

[Description("应用信息")]
public class AspectController : Controller
{
    /// <summary>
    ///     应用基础信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("/aspect")]
    public CommonResult ApplicationInfo()
    {
        var test = (IMachine)ServiceLocator.GetService(typeof(IMachine));
        test.Purchase(90);
        return CommonResult.Success();
    }
}