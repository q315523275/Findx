using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Findx.Linq;
using Findx.Module.EleAdmin.DTO;
using Findx.Module.EleAdmin.Models;
using System.ComponentModel;

namespace Findx.Module.EleAdmin.Areas.System.Controller
{
	/// <summary>
    /// 角色服务
    /// </summary>
	[Area("system")]
	[Route("api/[area]/role")]
    [Authorize]
	[Description("系统-角色")]
	public class SysRoleController: CrudControllerBase<SysRoleInfo, SetRoleRequest, QueryRoleRequest, Guid, Guid>
	{
		/// <summary>
		/// 构建查询条件
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
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
		[Description("系统-查看角色菜单")]
		public CommonResult Menu(Guid roleId)
        {
			var repo = HttpContext.RequestServices.GetRequiredService<IRepository<SysRoleMenuInfo>>();
			var menuRepo = HttpContext.RequestServices.GetRequiredService<IRepository<SysMenuInfo>>();

			var menuIdArray = repo.Select(whereExpression: x => x.RoleId == roleId, selectExpression: x => x.MenuId).Distinct();
			var menuList = menuRepo.Select<RoleMenuDto>();

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
		[Description("系统-设置角色菜单")]
		public CommonResult Menu(Guid roleId, [FromBody] List<Guid> req)
		{
			var repo = HttpContext.RequestServices.GetRequiredService<IRepository<SysRoleMenuInfo>>();

			repo.Delete(x => x.RoleId == roleId);
			var list = req.Select(x => new SysRoleMenuInfo
			{
				Id = Utils.SequentialGuid.Instance.Create(DatabaseType.MySql),
				MenuId = x,
				RoleId = roleId,
				TenantId = Tenant.TenantId.Value
			});
			if (list.Any())
				repo.Insert(list);

			return CommonResult.Success();
		}
	}
}

