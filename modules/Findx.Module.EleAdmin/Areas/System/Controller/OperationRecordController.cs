using Findx.AspNetCore.Mvc;
using Findx.Extensions;
using Findx.Linq;
using Findx.Module.EleAdmin.DTO;
using Findx.Module.EleAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Areas.System.Controller;

/// <summary>
/// 登录日志
/// </summary>
[Area("system")]
[Route("api/[area]/operation-record")]
[Authorize]
public class OperationRecordController :  QueryControllerBase<SysOperationRecordInfo, SysOperationRecordInfo, QueryOperationRecordRequest, Guid>
{
    /// <summary>
    /// 构建查询条件
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    protected override Expressionable<SysOperationRecordInfo> CreatePageWhereExpression(QueryOperationRecordRequest req)
    {
        var whereExp = ExpressionBuilder.Create<SysOperationRecordInfo>()
            .AndIF(!req.UserName.IsNullOrWhiteSpace(), x => x.UserName.Contains(req.UserName))
            .AndIF(!req.Nickname.IsNullOrWhiteSpace(), x => x.Nickname.Contains(req.Nickname))
            .AndIF(req.CreatedTimeStart.HasValue, x => x.CreatedTime >= req.CreatedTimeStart && x.CreatedTime < req.CreatedTimeEnd);
        return whereExp;
    }
}