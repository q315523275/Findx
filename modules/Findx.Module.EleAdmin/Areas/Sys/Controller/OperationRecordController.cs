using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Extensions;
using Findx.Linq;
using Findx.Module.EleAdmin.Dtos;
using Findx.Module.EleAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Areas.Sys.Controller;

/// <summary>
///     操作日志
/// </summary>
[Area("system")]
[Route("api/[area]/operation-record")]
[Authorize]
[Description("系统-操作日志")]
[ApiExplorerSettings(GroupName = "eleAdmin")]
[Tags("系统-操作日志")]
public class OperationRecordController : QueryControllerBase<SysOperationRecordInfo, SysOperationRecordInfo,
    QueryOperationRecordRequest, Guid>
{
    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    protected override Expressionable<SysOperationRecordInfo> CreatePageWhereExpression(QueryOperationRecordRequest req)
    {
        var whereExp = ExpressionBuilder.Create<SysOperationRecordInfo>()
            .AndIF(!req.UserName.IsNullOrWhiteSpace(), x => x.UserName.Contains(req.UserName))
            .AndIF(!req.Nickname.IsNullOrWhiteSpace(), x => x.Nickname.Contains(req.Nickname))
            .AndIF(req.CreatedTimeStart.HasValue,
                x => x.CreatedTime >= req.CreatedTimeStart && x.CreatedTime < req.CreatedTimeEnd);
        return whereExp;
    }
}