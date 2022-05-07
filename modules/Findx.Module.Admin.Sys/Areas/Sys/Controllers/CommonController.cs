using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Sys.Areas.Sys.Controllers
{
	public class CommonController: ControllerBase
    {
        /// <summary>
        /// html展示
        /// </summary>
        /// <returns></returns>
        [HttpGet("/findx_admin")]
        public async Task<IActionResult> AdminWeb([FromServices] IApplicationContext application)
        {
            using (var stream = System.IO.File.OpenRead(Path.Combine(application.RootPath, "wwwroot", "findx_admin", "index.html")))
            {
                using (var reader = new StreamReader(stream))
                {
                    return Content(await reader.ReadToEndAsync(), "text/html");
                }
            }
        }
    }
}

