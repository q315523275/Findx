using Findx.AspNetCore.Mvc;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 系统区域
    /// </summary>
    [Area("api/admin")]
    [Route("[area]/sysArea")]
    public class SysAreaController : QueryControllerBase<SysAreaInfo, SysAreaInfo, SysQuery, long>
    {
    }
}
