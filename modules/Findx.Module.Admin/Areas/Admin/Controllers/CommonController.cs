using Findx.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    [Area("System")]
    public class CommonController : AreaApiControllerBase
    {

        [HttpGet]
        public object GetControllers([FromServices] IApiInterfaceService api)
        {
            return api.GetAllController();
        }
        [HttpGet]
        public object GetActions([FromServices] IApiInterfaceService api)
        {
            return api.GetAllAction();
        }
    }
}
