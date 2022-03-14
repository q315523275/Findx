using Findx.AspNetCore.Mvc;
using Findx.Linq;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Models;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 短信
    /// </summary>
    [Area("api/admin")]
    [Route("[area]/sms")]
    public class SysSmsController : QueryControllerBase<SysSmsInfo, SysSmsInfo, SysSmsQuery, long>
    {
        /// <summary>
        /// 构建查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override Expressionable<SysSmsInfo> CreatePageWhereExpression(SysSmsQuery req)
        {
            return ExpressionBuilder.Create<SysSmsInfo>().AndIF(req.PhoneNumbers.IsNullOrWhiteSpace(), x => x.PhoneNumbers == req.PhoneNumbers)
                                                              .AndIF(req.Status.HasValue, x => x.Status == req.Status.Value)
                                                              .AndIF(req.Source.HasValue, x => x.Source == req.Source.Value);
        }
    }
}
