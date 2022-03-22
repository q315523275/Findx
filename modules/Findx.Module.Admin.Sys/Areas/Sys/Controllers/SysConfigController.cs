using Findx.AspNetCore.Mvc;
using Findx.Linq;
using Findx.Module.Admin.Sys.DTO;
using Findx.Module.Admin.Models;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Findx.Data;
using System.ComponentModel;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    /// <summary>
    /// 系统配置
    /// </summary>
    [Area("api/sys")]
    [Route("[area]/sysConfig")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysConfigController : CrudControllerBase<SysConfigInfo, SysConfigInfo, SysConfigRequest, SysConfigRequest, SysConfigQuery, long, long>
    {
        /// <summary>
        /// 构建查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override Expressionable<SysConfigInfo> CreatePageWhereExpression(SysConfigQuery request)
        {
            return ExpressionBuilder.Create<SysConfigInfo>().AndIF(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name))
                                                            .AndIF(!request.Code.IsNullOrWhiteSpace(), x => x.Code.Contains(request.Code))
                                                            .AndIF(!request.GroupCode.IsNullOrWhiteSpace(), x => x.GroupCode == request.GroupCode);
        }

        /// <summary>
        /// 构建排序规则
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override List<OrderByParameter<SysConfigInfo>> CreatePageOrderExpression(SysConfigQuery request)
        {
            var multiOrderBy = new List<OrderByParameter<SysConfigInfo>>();
            if (typeof(SysDictTypeInfo).IsAssignableTo(typeof(ISort)))
                multiOrderBy.Add(new OrderByParameter<SysConfigInfo> { Expression = it => (it as ISort).Sort, SortDirection = ListSortDirection.Ascending });
            multiOrderBy.Add(new OrderByParameter<SysConfigInfo> { Expression = it => it.Id, SortDirection = ListSortDirection.Ascending });
            return multiOrderBy;
        }
    }
}
