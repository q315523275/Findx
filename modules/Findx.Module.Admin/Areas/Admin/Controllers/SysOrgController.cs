using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 系统组织机构
    /// </summary>
    [Area("api/admin")]
    [Route("[area]/sysOrg")]
    public class SysOrgController : CrudControllerBase<SysOrgInfo, SysOrgInfo, SysOrgRequest, SysOrgRequest, SysOrgQuery, long, long>
    {
        //private readonly IRepository<SysOrgInfo> _repository;

        //public SysOrgController(IRepository<SysOrgInfo> repository)
        //{
        //    _repository = repository;
        //    _repository.AsTable(oldName => $"{oldName}_1");
        //}

        [HttpGet("dbinfo")]
        public CommonResult DbInfo([FromServices] IRepository<SysOrgInfo> repository)
        {
            return CommonResult.Success(new { table = repository.GetDbTableName(), columns = repository.GetDbColumnName() });
        }
    }
}
