using Microsoft.AspNetCore.Mvc;

namespace Findx.AspNetCore.Mvc
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public abstract class ApiControllerBase : ControllerBase
    {
    }
}
