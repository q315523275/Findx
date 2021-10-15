using Findx.AspNetCore.Mvc;
using Findx.Data;
using Microsoft.AspNetCore.Mvc;

namespace Findex.Module.Configuration.Areas.Configuraction
{
    /// <summary>
    /// 
    /// </summary>
    [Area("api/admin")]
    [Route("[area]/configuration")]
    public class ConfigurationController : ApiControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("management")]
        public CommonResult Management()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("detail")]
        public CommonResult Detail()
        {
            return null;
        }
    }
}
