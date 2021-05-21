using Findx.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "community")]
        public CommonResult<object> Common()
        {
            return CommonResult.Success<object>(null);
        }
    }
}
