using System.Collections.Generic;
using System.Threading.Tasks;
using Findx.Data;
using Findx.Module.Admin.Models;
using Findx.Extensions;
using Findx.Module.Admin.Enum;
using Findx.Module.Admin.Const;

namespace Findx.Module.Admin.Sys.Service
{
    /// <summary>
    /// 用户服务
    /// </summary>
    public class SysUserService : ISysUserService, Findx.DependencyInjection.IScopeDependency
    {
        private readonly IRepository<SysUserInfo> _repo_user;
        private readonly IRepository<SysEmpInfo> _repo_emp;
        private readonly IRepository<SysOrgInfo> _repo_org;
        private readonly IRepository<SysRoleInfo> _repo_role;
        private readonly IRepository<SysUserRoleInfo> _repo_user_role;
        private readonly IRepository<SysUserDataScopeInfo> _repo_user_data_scope;
        private readonly IRepository<SysRoleDataScopeInfo> _repo_role_data_scope;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="repo_user"></param>
        /// <param name="repo_emp"></param>
        /// <param name="repo_org"></param>
        /// <param name="repo_role"></param>
        /// <param name="repo_user_role"></param>
        /// <param name="repo_user_data_scope"></param>
        /// <param name="repo_role_data_scope"></param>
        public SysUserService(IRepository<SysUserInfo> repo_user, IRepository<SysEmpInfo> repo_emp, IRepository<SysOrgInfo> repo_org, IRepository<SysRoleInfo> repo_role, IRepository<SysUserRoleInfo> repo_user_role, IRepository<SysUserDataScopeInfo> repo_user_data_scope, IRepository<SysRoleDataScopeInfo> repo_role_data_scope)
        {
            _repo_user = repo_user;
            _repo_emp = repo_emp;
            _repo_org = repo_org;
            _repo_role = repo_role;
            _repo_user_role = repo_user_role;
            _repo_user_data_scope = repo_user_data_scope;
            _repo_role_data_scope = repo_role_data_scope;
        }

        /// <summary>
        /// 获取用户数据范围
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<long>> GetUserDataScopeIdList(long userId)
        {
            var empInfo = await _repo_emp.GetAsync(userId);
            var roleIdList = _repo_user_role.Select(x => x.UserId == userId, x => x.RoleId);
            var roles = _repo_role.Select(x => roleIdList.Contains(x.Id) && x.Status == 0, x => new { x.Id, x.Name, x.Code, x.DataScopeType });
            var dataScopes = _repo_user_data_scope.Select(x => x.UserId == userId, x => x.OrgId); // 用户直接分配范围

            if (empInfo != null && roles.Count > 0)
            {
                // 自定义的数据范围角色ID集合
                var customDataScopeRoleIdList = new List<long>();
                var strongerDataScopeType = DataScopeTypeEnum.SELF.To<int>();
                foreach (var sysRole in roles)
                {
                    if (DataScopeTypeEnum.DEFINE.To<int>() == sysRole.DataScopeType)
                    {
                        // 自定义的数据范围
                        customDataScopeRoleIdList.Add(sysRole.Id);
                    }
                    else
                    {
                        // 限定自身机构数据范围
                        if (sysRole.DataScopeType <= strongerDataScopeType)
                        {
                            strongerDataScopeType = sysRole.DataScopeType;
                        }
                    }
                }
                // 自定义数据范围的角色对应的数据范围
                var dataScopes2 = _repo_role_data_scope.Select(it => customDataScopeRoleIdList.Contains(it.RoleId), it => it.OrgId);
                // 角色中拥有最大数据范围类型的数据范围
                var dataScopes3 = new List<long>();
                // 如果是范围类型是全部数据，则获取当前系统所有的组织架构id
                if (DataScopeTypeEnum.ALL.To<int>() == strongerDataScopeType)
                {
                    dataScopes3 = _repo_org.Select(it => it.Status == 0, it => it.Id);
                }
                // 如果范围类型是本部门及以下部门，则查询本节点和子节点集合，包含本节点
                else if (DataScopeTypeEnum.DEPT_WITH_CHILD.To<int>() == strongerDataScopeType)
                {
                    var likeValue = $"{SymbolConst.LEFT_SQUARE_BRACKETS}{empInfo.OrgId}{SymbolConst.RIGHT_SQUARE_BRACKETS}";
                    dataScopes3 = _repo_org.Select(it => it.Pids.Contains(likeValue) && it.Status == 0, it => it.Id);
                    dataScopes3.Add(empInfo.OrgId);
                }
                // 如果数据范围是本部门，不含子节点，则直接返回本部门
                else if (DataScopeTypeEnum.DEPT.To<int>() == strongerDataScopeType)
                {
                    dataScopes3.Add(empInfo.OrgId);
                }
                dataScopes.AddRange(dataScopes2);
                dataScopes.AddRange(dataScopes3);
            }

            return dataScopes;
        }
    }
}

