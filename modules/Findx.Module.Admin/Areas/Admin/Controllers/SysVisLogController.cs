using Findx.AspNetCore.Mvc;
using Findx.Linq;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Models;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Findx.Data;
using System.Collections.Generic;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 访问日志
    /// </summary>
    [Area("api/admin")]
    [Route("[area]/sysVisLog")]
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

    }
}
