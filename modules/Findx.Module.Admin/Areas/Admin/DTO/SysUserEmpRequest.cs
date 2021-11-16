using System.Collections.Generic;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
    /// <summary>
    /// 员工信息参数
    /// </summary>
    public class SysUserEmpRequest
    {
        /// <summary>
        /// 员工Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 工号
        /// </summary>
        public string JobNum { get; set; }

        /// <summary>
        /// 机构Id
        /// </summary>
        public string OrgId { get; set; }

        /// <summary>
        /// 机构名称
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 附属机构
        /// </summary>
        public List<SysEmpExtOrgPosDTO> ExtIds { get; set; } = new List<SysEmpExtOrgPosDTO>();

        /// <summary>
        /// 职位集合
        /// </summary>
        public List<long> PosIdList { get; set; } = new List<long>();
    }
}
