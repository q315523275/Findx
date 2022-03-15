using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Linq;
using Findx.Module.Admin.DTO;
using Findx.Module.Admin.Models;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    /// <summary>
    /// 访问日志
    /// </summary>
    [Area("api/sys")]
    [Route("[area]/sysVisLog")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysVisLogController : CrudControllerBase<SysVisLogInfo, SysVisLogInfo, SysVisLogInfo, SysVisLogInfo, SysVisLogQuery, long, long>
    {
        protected override Expressionable<SysVisLogInfo> CreatePageWhereExpression(SysVisLogQuery request)
        {
            return ExpressionBuilder.Create<SysVisLogInfo>().AndIF(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name))
                                                            .AndIF(!request.Success.IsNullOrWhiteSpace(), x => x.Success == request.Success)
                                                            .AndIF(request.VisType > 0, x => x.VisType == (int)request.VisType)
                                                            .AndIF(request.SearchBeginTime.HasValue && request.SearchEndTime.HasValue, x => request.SearchBeginTime < x.VisTime && request.SearchEndTime > x.VisTime);
        }

        /// <summary>
        /// 清空访问日志
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete")]
        public override async Task<CommonResult> DeleteAsync([FromBody] List<DeleteParam<long>> request)
        {
            var repo = GetRepository<SysVisLogInfo>();
            await repo.DeleteAsync();
            return CommonResult.Success();
        }


        /// <summary>
        /// 用户导出
        /// </summary>
        /// <returns></returns>
        [HttpGet("export")]
        public async Task<IActionResult> Export()
        {
            var list = GetRepository<SysVisLogInfo>().Select();

            //var memoryStream = new MemoryStream();
            //memoryStream.SaveAs(list);
            //memoryStream.Seek(0, SeekOrigin.Begin);
            //return await Task.FromResult(new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            //{
            //    FileDownloadName = "user.xlsx"
            //});
            return null;
        }
    }
}
