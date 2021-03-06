using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 系统组织机构表
    /// </summary>
    public partial class SysOrg
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long? CreateUser { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 父id
        /// </summary>
        public long Pid { get; set; }

        /// <summary>
        /// 父ids
        /// </summary>
        public string Pids { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        public sbyte Status { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public long? UpdateUser { get; set; }

    }

}
