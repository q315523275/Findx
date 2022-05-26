using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Findx.Admin.Module.System.DTO;
using Findx.Admin.Module.System.Models;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Linq;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Findx.Admin.Module.System.Areas.System.Controller
{
	/// <summary>
    /// 用户服务
    /// </summary>
	[Area("system")]
	[Route("api/[area]/menu")]
    [Authorize]
    public class SysMeunController: CrudControllerBase<SysMenuInfo, SetMenuRequest, QueryMenuRequest, int, int>
	{
        protected override Expressionable<SysMenuInfo> CreatePageWhereExpression(QueryMenuRequest request)
        {
            var whereExp = ExpressionBuilder.Create<SysMenuInfo>()
                                            .AndIF(!request.Title.IsNullOrWhiteSpace(), x => x.Title.Contains(request.Title))
                                            .AndIF(!request.Path.IsNullOrWhiteSpace(), x => x.Path.Contains(request.Path))
                                            .AndIF(!request.Authority.IsNullOrWhiteSpace(), x => x.Authority.Contains(request.Authority));
            return whereExp;
        }

        public override Task<CommonResult> ListAsync([FromQuery] QueryMenuRequest request)
        {
            request.PageSize = 9999;
            return base.ListAsync(request);
        }
    }
}

