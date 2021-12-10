using Findx.Data;
using Findx.Scheduling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Findx.WebHost.Controllers
{
    /// <summary>
    /// 应用信息
    /// </summary>
    public class ApplicationController : Controller
    {
        /// <summary>
        /// 健康检查地址
        /// </summary>
        /// <returns></returns>
        [HttpGet("/health")]
        public JsonResult Health()
        {
            return new JsonResult(new { status = "UP" });
        }

        /// <summary>
        /// 应用基础信息
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        [HttpGet("/applicationInfo")]
        public CommonResult ApplicationInfo([FromServices] IApplicationInstanceInfo instance)
        {
            return CommonResult.Success(instance);
        }

        /// <summary>
        /// 配置信息
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        [HttpGet("/configuration")]
        public CommonResult Configuration([FromServices] IConfiguration configuration, [FromServices] IOptionsMonitor<SchedulerOptions> optionsMonitor)
        {
            return CommonResult.Success<object>(optionsMonitor.CurrentValue.ToString());
        }

    }
}
