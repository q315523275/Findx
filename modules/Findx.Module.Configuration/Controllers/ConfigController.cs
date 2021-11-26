using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Findex.Module.Configuration.Controllers
{
    /// <summary>
    /// 应用配置查询
    /// </summary>
    public class ConfigController: ControllerBase
    {
        /// <summary>
        /// 配置查询
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="group"></param>
        /// <param name="namespaceName"></param>
        /// <param name="version"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        [HttpGet("/configs/{appId}/{group}/{namespaceName}")]
        public IActionResult Configs([Required] string appId, [Required] string group, [Required] string namespaceName, long version, [Required] string sign)
        {

            return null;
        }
    }
}
