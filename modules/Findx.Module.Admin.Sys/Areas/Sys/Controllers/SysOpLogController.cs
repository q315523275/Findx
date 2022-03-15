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
using System.Threading.Tasks;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    /// <summary>
    /// 操作日志
    /// </summary>
    [Area("api/sys")]
    [Route("[area]/sysOpLog")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysOpLogController : CrudControllerBase<SysOpLogInfo, SysOpLogInfo, SysOpLogInfo, SysOpLogInfo, SysOpLogQuery, long, long>
    {
        /// <summary>
        /// 构建查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override Expressionable<SysOpLogInfo> CreatePageWhereExpression(SysOpLogQuery request)
        {
            return ExpressionBuilder.Create<SysOpLogInfo>().AndIF(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name))
                                                            .AndIF(!request.Success.IsNullOrWhiteSpace(), x => x.Success == request.Success)
                                                            .AndIF(!request.ReqMethod.IsNullOrWhiteSpace(), x => x.ReqMethod == request.ReqMethod)
                                                            .AndIF(request.SearchBeginTime.HasValue && request.SearchEndTime.HasValue, x => request.SearchBeginTime < x.OpTime && request.SearchEndTime > x.OpTime);
        }

        /// <summary>
        /// 清空访问日志
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete")]
        public override async Task<CommonResult> DeleteAsync([FromBody] List<DeleteParam<long>> request)
        {
            var repo = GetRepository<SysOpLogInfo>();
            await repo.DeleteAsync();
            return CommonResult.Success();
        }
    }
}
