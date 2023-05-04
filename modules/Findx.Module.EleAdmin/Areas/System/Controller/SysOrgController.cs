using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Linq;
using Findx.Module.EleAdmin.Dtos;
using Findx.Module.EleAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Areas.System.Controller;

/// <summary>
///     机构服务
/// </summary>
[Area("system")]
[Route("api/[area]/org")]
[Authorize]
[Description("系统-机构")]
[ApiExplorerSettings(GroupName = "eleAdmin")]
[Tags("系统-机构")]
public class SysOrgController : CrudControllerBase<SysOrgInfo, SetOrgRequest, QueryOrgRequest, Guid, Guid>
{
    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override Expressionable<SysOrgInfo> CreatePageWhereExpression(QueryOrgRequest request)
    {
        var whereExp = ExpressionBuilder.Create<SysOrgInfo>()
            .AndIF(request.Pid != null && request.Pid != Guid.Empty, x => x.ParentId == request.Pid)
            .AndIF(!request.Keywords.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Keywords));
        return whereExp;
    }

    /// <summary>
    ///     构建排序
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override List<OrderByParameter<SysOrgInfo>> CreatePageOrderExpression(QueryOrgRequest request)
    {
        return ExpressionBuilder.CreateOrder<SysOrgInfo>().OrderBy(x => x.Sort).ToSort();
    }
}