using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 系统参数配置表
    /// </summary>
    public partial class SysConfig
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
        /// 常量所属分类的编码，来自于“常量的分类”字典
        /// </summary>
        public string GroupCode { get; set; } = string.Empty;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        public sbyte Status { get; set; }

        /// <summary>
        /// 是否是系统参数（Y-是，N-否）
        /// </summary>
        public string SysFlag { get; set; } = string.Empty;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public long? UpdateUser { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; } = string.Empty;

    }

}
