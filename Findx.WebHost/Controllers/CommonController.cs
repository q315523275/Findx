using Findx.Data;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        [HttpGet]
        public CommonResult<object> Common()
        {
            return null;
        }
    }
}
