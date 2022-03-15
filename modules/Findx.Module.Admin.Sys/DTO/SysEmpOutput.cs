using System.Collections.Generic;

namespace Findx.Module.Admin.DTO
{
    /// <summary>
    /// 员工信息参数
    /// </summary>
    public class SysEmpOutput
    {
        /// <summary>
        /// 工号
        /// </summary>
        public string JobNum { get; set; }

        /// <summary>
        /// 机构id
        /// </summary>
        public string OrgId { get; set; }

        /// <summary>
        /// 机构名称
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 机构与职位信息
        /// </summary>
        public List<SysEmpExtOrgPosOutput> ExtOrgPos { get; set; } = new List<SysEmpExtOrgPosOutput>();

        /// <summary>
        /// 职位信息
        /// </summary>
        public List<SysEmpPosOutput> Positions { get; set; } = new List<SysEmpPosOutput>();
    }
}
