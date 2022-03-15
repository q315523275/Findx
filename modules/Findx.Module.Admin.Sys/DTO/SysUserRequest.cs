using System.Collections.Generic;
using Findx.Data;

namespace Findx.Module.Admin.DTO
{
    /// <summary>
    /// 系统用户通用请求
    /// </summary>
    public class SysUserRequest: IRequest
	{
		/// <summary>
        /// 用户编号
        /// </summary>
		public long Id { set; get; }

        /// <summary>
        /// 角色编号集合
        /// </summary>
        public List<long> GrantRoleIdList { set; get; }

        /// <summary>
        /// 机构编号集合
        /// </summary>
        public List<long> GrantOrgIdList { set; get; }
    }
}

