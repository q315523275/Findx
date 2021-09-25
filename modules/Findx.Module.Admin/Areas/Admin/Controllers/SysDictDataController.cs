using Findx.AspNetCore.Mvc;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    [Area("api/admin")]
    [Route("[area]/sysDictData")]
    public class SysDictDataController : CrudControllerBase<SysDictDataInfo, SysDictDataInfo, SysDictDataRequest, SysDictDataRequest, SysQuery, long, long>
    {
    }
}
