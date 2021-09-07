using Findx.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("[area]/[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "admin")]
    public class AccountController : AreaApiControllerBase
    {

    }
}
