using Findx.Data;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
    /// <summary>
    /// 用户查询入参
    /// </summary>
    public class SysUserQuery : PageBase
    {
        /// <summary>
        /// 查询关键词
        /// </summary>
        public string SearchValue { get; set; }

        /// <summary>
        /// 查询状态
        /// </summary>
        public int SearchStatus { get; set; } = -1;

        /// <summary>
        /// 机构信息
        /// </summary>
        public SysUserQueryOrg SysEmpParam { get; set; }
    }

    public class SysUserQueryOrg
    {
        public string OrgId { get; set; }
    }
}
