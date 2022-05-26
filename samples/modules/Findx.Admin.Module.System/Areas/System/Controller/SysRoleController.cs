using System;
using System.Linq;
using System.Threading.Tasks;
using Findx.Admin.Module.System.DTO;
using Findx.Admin.Module.System.Models;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Security;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Findx.Linq;

namespace Findx.Admin.Module.System.Areas.System.Controller
{
	/// <summary>
    /// 角色服务
    /// </summary>
	[Area("system")]
	[Route("api/[area]/role")]
    [Authorize]
	public class SysRoleController: CrudControllerBase<SysRoleInfo, SetRoleRequest, QueryRoleRequest, int, int>
	{
		protected override Expressionable<SysRoleInfo> CreatePageWhereExpression(QueryRoleRequest request)
		{
			var whereExp = ExpressionBuilder.Create<SysRoleInfo>()
											.AndIF(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name))
											.AndIF(!request.Code.IsNullOrWhiteSpace(), x => x.Code.Contains(request.Code))
											.AndIF(!request.Comments.IsNullOrWhiteSpace(), x => x.Comments.Contains(request.Comments));
			return whereExp;
		}

		/// <summary>
		/// 查询角色对应菜单
		/// </summary>
		/// <param name="roleId"></param>
		/// <returns></returns>
		[HttpGet("menu/{roleId}")]
		public CommonResult Menu(int roleId)
        {
			var repo = HttpContext.RequestServices.GetService<IRepository<SysRoleMenuInfo>>();
			var menu_repo = HttpContext.RequestServices.GetService<IRepository<SysMenuInfo>>();

			var menuIdArray = repo.Select(whereExpression: x => x.RoleId == roleId, selectExpression: x => x.MenuId);

			var menuList = menu_repo.Select<RoleMenuDto>();

			menuList.ForEach(x =>
			{
				if (menuIdArray.Contains(x.Id))
					x.Checked = true;
			});

			return CommonResult.Success(menuList);
		}

		/// <summary>
        /// 设置角色对应菜单
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
		[HttpPut("menu/{roleId}")]
		public CommonResult Menu(int roleId, [FromBody] List<int> req)
		{
			var repo = HttpContext.RequestServices.GetService<IRepository<SysRoleMenuInfo>>();

			repo.Delete(x => x.RoleId == roleId);

			var list = req.Select(x => new SysRoleMenuInfo { MenuId = x, RoleId = roleId, TenantId = Findx.Data.Tenant.TenantId.Value });
			if (list.Count() > 0)
				repo.Insert(list);

			return CommonResult.Success();
		}
	}
}

