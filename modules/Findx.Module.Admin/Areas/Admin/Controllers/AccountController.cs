using Findx.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("[area]/[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "admin")]
    public class AccountController : AreaApiControllerBase
    {

    }
}
