using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Linq;
using Findx.Extensions;
using Findx.Module.EleAdmin.DTO;
using Findx.Module.EleAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Findx.Module.EleAdmin.Areas.System.Controller
{
	/// <summary>
    /// 菜单服务
    /// </summary>
	[Area("system")]
	[Route("api/[area]/menu")]
    [Authorize]
    public class SysMeunController: CrudControllerBase<SysMenuInfo, SetMenuRequest, QueryMenuRequest, Guid, Guid>
	{
        /// <summary>
        /// 构建查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override Expressionable<SysMenuInfo> CreatePageWhereExpression(QueryMenuRequest request)
        {
            var whereExp = ExpressionBuilder.Create<SysMenuInfo>()
                                            .AndIF(!request.Title.IsNullOrWhiteSpace(), x => x.Title.Contains(request.Title))
                                            .AndIF(!request.Path.IsNullOrWhiteSpace(), x => x.Path.Contains(request.Path))
                                            .AndIF(request.ParentId.HasValue , x => x.ParentId == request.ParentId)
                                            .AndIF(!request.Authority.IsNullOrWhiteSpace(), x => x.Authority.Contains(request.Authority));
            return whereExp;
        }

        /// <summary>
        /// 列表查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Task<CommonResult> ListAsync([FromQuery] QueryMenuRequest request)
        {
            request.PageSize = 9999;
            return base.ListAsync(request);
        }
    }
}

