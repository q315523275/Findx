using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;
using Findx.Security;
using Findx.Module.Admin.Internals;
using Findx.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Findx.Utils;
using Findx.Linq;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 系统菜单
    /// </summary>
    [Area("api/admin")]
    [Route("[area]/sysMenu")]
    [Authorize]
    public class SysMenuController : CrudControllerBase<SysMenuInfo, SysMenuOutput, SysMenuInfo, SysMenuInfo, SysMenuQuery, long, long>
    {
        /// <summary>
        /// 切换应用菜单
        /// </summary>
        /// <param name="input"></param>
        /// <param name="fsql"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        [HttpPost("change")]
        public CommonResult Change([FromBody] ChangeAppMenuInput input, [FromServices] IFreeSql fsql, [FromServices] IPrincipal principal)
        {
            var userId = principal.Identity.GetUserId<long>();

            var userInfo = fsql.Select<SysUserInfo>(userId).First();
            if (userInfo == null)
                return CommonResult.Fail("401", "登录信息失效,请重新登录");

            var roles = fsql.Select<SysRoleInfo, SysUserRoleInfo>().InnerJoin((a, b) => a.Id == b.RoleId && b.UserId == userId).Where((a, b) => a.Status == 0)
                            .ToList((a, b) => new { a.Id, a.Name, a.Code, a.DataScopeType });
            var roleIdList = roles.Select(it => it.Id).ToList();

            var menuList = fsql.Select<SysMenuInfo, SysRoleMenuInfo>().LeftJoin((a, b) => a.Id == b.MenuId)
                               .WhereIf(!userInfo.IsSuperAdmin(), (a, b) => roleIdList.Contains(b.RoleId))
                               .WhereIf(userInfo.IsSuperAdmin(), (a, b) => a.Weight != 2)
                               .Where((a, b) => a.Application == input.Application && a.Type != 2 && a.Status == 0)
                               .OrderBy((a, b) => a.Sort)
                               .ToList((a, b) => new LoginUserMenuDTO
                               {
                                   OpenType = a.OpenType,
                                   Id = a.Id,
                                   Pid = a.Pid,
                                   Name = a.Code,
                                   Component = a.Component,
                                   Redirect = a.Redirect,
                                   Path = a.Router,
                                   Hidden = a.Visible == "N",
                                   Meta = new LoginUserMenuMetaDTO
                                   {
                                       Title = a.Name,
                                       Icon = a.Icon,
                                       Show = a.Visible == "Y",
                                       Link = a.Link
                                   }
                               });

            foreach (var item in menuList)
            {
                if (MenuOpenTypeEnum.OUTER.To<int>() == item.OpenType)
                {
                    item.Meta.Target = "_blank";
                    item.Path = item.Meta.Link;
                    item.Redirect = item.Meta.Link;
                }
            }

            return CommonResult.Success(menuList);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override async Task<CommonResult> ListAsync([FromQuery] SysMenuQuery request)
        {
            Check.NotNull(request, nameof(request));

            var repo = GetRepository<SysMenuInfo>();

            Check.NotNull(repo, nameof(repo));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var list = await repo.SelectAsync<SysMenuOutput>(whereExpression?.ToExpression());

            return CommonResult.Success(new TreeBuilder<SysMenuOutput, long>().Build(list, 0));
        }

        protected override Expressionable<SysMenuInfo> CreatePageWhereExpression(SysMenuQuery request)
        {
            return ExpressionBuilder.Create<SysMenuInfo>().AndIF(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name))
                                                            .AndIF(!request.Application.IsNullOrWhiteSpace(), x => x.Code.Contains(request.Application))
                                                            .And(x => x.Status == CommonStatusEnum.ENABLE.To<int>());
        }
    }
}
