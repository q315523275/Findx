using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Linq;
using Findx.Module.EleAdmin.Dtos;
using Findx.Module.EleAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Controller;

/// <summary>
///     机构服务
/// </summary>
[Area("system")]
[Route("api/[area]/org")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdmin"), Tags("系统-机构"), Description("系统-机构")]
public class SysOrgController : CrudControllerBase<SysOrgInfo, SetOrgRequest, QueryOrgRequest, Guid, Guid>
{
    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override Expression<Func<SysOrgInfo, bool>> CreatePageWhereExpression(QueryOrgRequest request)
    {
        var whereExp = PredicateBuilder.New<SysOrgInfo>()
                                       .AndIf(request.Pid != null && request.Pid != Guid.Empty, x => x.ParentId == request.Pid)
                                       .AndIf(!request.Keywords.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Keywords))
                                       .Build();
        return whereExp;
    }

    /// <summary>
    ///     构建排序
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override List<OrderByParameter<SysOrgInfo>> CreatePageOrderExpression(QueryOrgRequest request)
    {
        return DataSortBuilder.New<SysOrgInfo>().OrderBy(x => x.Sort).Build();
    }
}