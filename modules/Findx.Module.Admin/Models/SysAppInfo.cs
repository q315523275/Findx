using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 系统应用表
    /// </summary>
    public partial class SysApp
    {

        /// <summary>
        /// 主键id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 是否默认激活（Y-是，N-否）
        /// </summary>
        public string Active { get; set; } = string.Empty;

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
        /// 应用名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        public sbyte Status { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public long? UpdateUser { get; set; }

    }

}
