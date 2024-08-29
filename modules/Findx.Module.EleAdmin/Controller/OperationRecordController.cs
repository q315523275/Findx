using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Expressions;
using Findx.Extensions;
using Findx.Module.EleAdmin.Dtos;
using Findx.Module.EleAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Controller;

/// <summary>
///     操作日志
/// </summary>
[Area("system")]
[Route("api/[area]/operation-record")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdmin"), Tags("系统-操作日志"), Description("系统-操作日志")]
public class OperationRecordController : QueryControllerBase<SysOperationRecordInfo, SysOperationRecordInfo, QueryOperationRecordRequest, Guid>
{
    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    protected override Expression<Func<SysOperationRecordInfo, bool>> CreateWhereExpression(QueryOperationRecordRequest req)
    {
        var whereExp = PredicateBuilder.New<SysOperationRecordInfo>()
                                       .AndIf(!req.UserName.IsNullOrWhiteSpace(), x => x.UserName.Contains(req.UserName))
                                       .AndIf(!req.Nickname.IsNullOrWhiteSpace(), x => x.Nickname.Contains(req.Nickname))
                                       .AndIf(req.CreatedTimeStart.HasValue, x => x.CreatedTime >= req.CreatedTimeStart && x.CreatedTime < req.CreatedTimeEnd)
                                       .Build();
        return whereExp;
    }
}