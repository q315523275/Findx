using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Findx.Data;
using Findx.Module.Admin.Models;
using Findx.Security;
using Findx.Extensions;
using Findx.Module.Admin.Internals;

namespace Findx.Module.Admin.Service
{
    /// <summary>
    /// 用户服务
    /// </summary>
    public class SysUserService : ISysUserService
    {
        private readonly IRepository<SysUserInfo> _repo_user;
        private readonly IRepository<SysEmpInfo> _repo_emp;
        private readonly IRepository<SysOrgInfo> _repo_org;
        private readonly IRepository<SysUserDataScopeInfo> _repo_user_data_scope;
        private readonly IRepository<SysRoleDataScopeInfo> _repo_role_data_scope;
        private readonly ICurrentUser _currentUser;

        public async Task<List<long>> GetUserDataScopeIdList()
        {
            throw new NotImplementedException();
        }

        public Task<List<long>> GetUserDataScopeIdList(long userId)
        {
            throw new NotImplementedException();
        }
    }
}

