using System.Collections.Generic;
using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Linq;
using Findx.Extensions;
using Findx.Module.Admin.Sys.DTO;
using Findx.Module.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Findx.Module.Admin.Sys.Filters;
using Findx.Security;
using Findx.Module.Admin.Enum;
using System.Linq;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    /// <summary>
    /// 角色
    /// </summary>
    [Area("api/sys")]
    [Route("[area]/sysRole")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysRoleController : CrudControllerBase<SysRoleInfo, SysRoleInfo, SysRoleInfo, SysRoleInfo, SysRoleQuery, long, long>
    {

        /// <summary>
        /// 构建查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override Expressionable<SysRoleInfo> CreatePageWhereExpression(SysRoleQuery request)
        {
            return ExpressionBuilder.Create<SysRoleInfo>().AndIF(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name))
                                                          .AndIF(!request.Code.IsNullOrWhiteSpace(), x => x.Code.Contains(request.Code));
        }

        /// <summary>
        /// 构建排序规则
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override List<OrderByParameter<SysRoleInfo>> CreatePageOrderExpression(SysRoleQuery request)
        {
            var multiOrderBy = new List<OrderByParameter<SysRoleInfo>>();
            if (typeof(SysRoleInfo).IsAssignableTo(typeof(ISort)))
                multiOrderBy.Add(new OrderByParameter<SysRoleInfo> { Expression = it => (it as ISort).Sort, SortDirection = ListSortDirection.Ascending });
            multiOrderBy.Add(new OrderByParameter<SysRoleInfo> { Expression = it => it.Id, SortDirection = ListSortDirection.Ascending });
            return multiOrderBy;
        }

        /// <summary>
        /// 查询角色拥有的菜单ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="repo"></param>
        /// <returns></returns>
        [HttpGet("ownMenu")]
        public CommonResult OwnMenu([FromQuery] long id, [FromServices] IRepository<SysRoleMenuInfo> repo)
        {
            var ids = repo.Select(x => x.RoleId == id, x => x.MenuId);

            return CommonResult.Success(ids);
        }

        /// <summary>
        /// 设置角色菜单
        /// </summary>
        /// <returns></returns>
        [HttpPost("grantMenu")]
        public CommonResult GrantMenu([FromBody] SysRoleGrantMenuRequest req, [FromServices] IRepository<SysRoleMenuInfo> repo)
        {
            repo.Delete(x => x.RoleId == req.Id);
            var list = new List<SysRoleMenuInfo>();
            req.GrantMenuIdList?.ForEach(x =>
            {
                list.Add(new SysRoleMenuInfo { MenuId = x, RoleId = req.Id, Id = Findx.Utils.SnowflakeId.Default().NextId() });
            });
            if (list.Count > 0)
                repo.Insert(list);

            return CommonResult.Success();
        }

        /// <summary>
        /// 查询角色拥有的数据ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="repo"></param>
        /// <returns></returns>
        [HttpGet("ownData")]
        public CommonResult OwnData([FromQuery] long id, [FromServices] IRepository<SysRoleDataScopeInfo> repo)
        {
            var ids = repo.Select(x => x.RoleId == id, x => x.OrgId);

            return CommonResult.Success(ids);
        }

        /// <summary>
        /// 设置角色数据范围
        /// </summary>
        /// <returns></returns>
        [DataScope]
        [HttpPost("grantData")]
        public CommonResult GrantData([FromBody] SysRoleGrantDataRequest req, [FromServices] ICurrentUser currentUser)
        {
            // 如果登录用户不是超级管理员，则进行数据权限校验
            if (!currentUser.IsSuperAdmin())
            {
                var dataScopeType = req.DataScopeType;
                // 如果授权的角色的数据范围类型为全部，则没权限，只有超级管理员有
                if (DataScopeTypeEnum.ALL.To<int>() == dataScopeType)
                {
                    return CommonResult.Fail("D4003", "没有权限操作该数据，请联系管理员");
                }
                // 如果授权的角色数据范围类型为自定义，则要判断授权的数据范围是否在自己的数据范围内
                if (DataScopeTypeEnum.DEFINE.To<int>() == dataScopeType)
                {
                    List<long> dataScope = currentUser.DataScope();
                    // 要授权的数据范围列表
                    List<long> grantOrgIdList = req.GrantOrgIdList;
                    if (!grantOrgIdList.IsNullOrEmpty())
                    {
                        // 数据范围为空
                        if (dataScope.IsNullOrEmpty())
                        {
                            return CommonResult.Fail("D4003", "没有权限操作该数据，请联系管理员");
                        }
                        else if (dataScope.Except(grantOrgIdList).Any()) // 存在差集
                        {
                            // 所要授权的数据不在自己的数据范围内
                            return CommonResult.Fail("D4003", "没有权限操作该数据，请联系管理员");
                        }
                    }
                }
            }

            var repo_role = GetRepository<SysRoleInfo>();
            var repo_data = GetRepository<SysRoleDataScopeInfo>();

            repo_data.Delete(x => x.RoleId == req.Id);

            var list = new List<SysRoleDataScopeInfo>();
            req.GrantOrgIdList?.ForEach(x =>
            {
                list.Add(new SysRoleDataScopeInfo { OrgId = x, RoleId = req.Id, Id = Utils.SnowflakeId.Default().NextId() });
            });
            if (list.Count > 0)
                repo_data.Insert(list);

            repo_role.UpdateColumns(x => new SysRoleInfo { DataScopeType = req.DataScopeType }, x => x.Id == req.Id);

            return CommonResult.Success();
        }
    }
}
