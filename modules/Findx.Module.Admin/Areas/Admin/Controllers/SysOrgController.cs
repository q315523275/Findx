using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;
using Findx.Security;
using Findx.Extensions;
using Findx.Module.Admin.Internals;
using System.Collections.Generic;
using System.Linq;
using Findx.Utils;
using Findx.Linq;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 系统组织机构
    /// </summary>
    [Area("api/admin")]
    [Route("[area]/sysOrg")]
    public class SysOrgController : CrudControllerBase<SysOrgInfo, SysOrgInfo, SysOrgRequest, SysOrgRequest, SysOrgQuery, long, long>
    {
        /// <summary>
        /// 树形数据查询
        /// </summary>
        /// <param name="fsql"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        [HttpGet("tree")]
        public CommonResult Tree([FromServices] IFreeSql fsql, [FromServices] IPrincipal principal)
        {
            var userId = principal.Identity.GetUserId<long>();

            var userInfo = fsql.Select<SysUserInfo>(userId).First();
            if (userInfo == null)
                return CommonResult.Fail("401", "登录信息失效,请重新登录");

            var dataScopes = new List<long>();
            if (!userInfo.IsSuperAdmin())
            {
                var empInfo = fsql.Select<SysEmpInfo>(userId).First();
                dataScopes = fsql.Select<SysUserDataScopeInfo>().Where(it => it.UserId == userId).ToList(it => it.OrgId); // 用户直接分配范围
                var roles = fsql.Select<SysRoleInfo, SysUserRoleInfo>().InnerJoin((a, b) => a.Id == b.RoleId && b.UserId == userId).Where((a, b) => a.Status == 0)
                                .ToList((a, b) => new { a.Id, a.Name, a.Code, a.DataScopeType });
                var roleIdList = roles.Select(it => it.Id).ToList();
                if (empInfo != null && roles.Count > 0)
                {
                    var customDataScopeRoleIdList = new List<long>();
                    var strongerDataScopeType = DataScopeTypeEnum.SELF.To<int>();
                    foreach (var sysRole in roles)
                    {
                        if (DataScopeTypeEnum.DEFINE.To<int>() == sysRole.DataScopeType)
                        {
                            customDataScopeRoleIdList.Add(sysRole.Id);
                        }
                        else
                        {
                            if (sysRole.DataScopeType <= strongerDataScopeType)
                            {
                                strongerDataScopeType = sysRole.DataScopeType;
                            }
                        }
                    }
                    // 自定义数据范围的角色对应的数据范围
                    var dataScopes2 = fsql.Select<SysRoleDataScopeInfo>().Where(it => customDataScopeRoleIdList.Contains(it.RoleId)).ToList(it => it.OrgId);
                    // 角色中拥有最大数据范围类型的数据范围
                    var dataScopes3 = new List<long>();
                    // 如果是范围类型是全部数据，则获取当前系统所有的组织架构id
                    if (DataScopeTypeEnum.ALL.To<int>() == strongerDataScopeType)
                    {
                        dataScopes3 = fsql.Select<SysOrgInfo>().Where(it => it.Status == 0).ToList(it => it.Id);
                    }
                    // 如果范围类型是本部门及以下部门，则查询本节点和子节点集合，包含本节点
                    else if (DataScopeTypeEnum.DEPT_WITH_CHILD.To<int>() == strongerDataScopeType)
                    {
                        var likeValue = $"{SymbolConstant.LEFT_SQUARE_BRACKETS}{empInfo.OrgId}{SymbolConstant.RIGHT_SQUARE_BRACKETS}";
                        dataScopes3 = fsql.Select<SysOrgInfo>().Where(it => it.Pids.Contains(likeValue) && it.Status == 0).ToList(it => it.Id);
                    }
                    // 如果数据范围是本部门，不含子节点，则直接返回本部门
                    else if (DataScopeTypeEnum.DEPT.To<int>() == strongerDataScopeType)
                    {
                        dataScopes3.Add(empInfo.OrgId);
                    }
                    dataScopes.AddRange(dataScopes2);
                    dataScopes.AddRange(dataScopes3);
                }
            }

            var orgList = fsql.Select<SysOrgInfo>()
                              .WhereIf(dataScopes.Count > 0, x => dataScopes.Contains(x.Id))
                              .Where(x => x.Status == 0).OrderBy(x => x.Sort)
                              .ToList(x => new OrgTreeNode
                              {
                                  Id = x.Id,
                                  ParentId = x.Pid,
                                  Title = x.Name,
                                  Value = x.Id.ToString(),
                                  Weight = x.Sort
                              });

            return CommonResult.Success(new TreeBuilder<OrgTreeNode, long>().Build(orgList, 0));
        }
    }
}
