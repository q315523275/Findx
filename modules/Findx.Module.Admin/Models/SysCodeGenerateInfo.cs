using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 代码生成基础配置
    /// </summary>
    public partial class SysCodeGenerate
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 作者姓名
        /// </summary>
        public string AuthorName { get; set; } = string.Empty;

        /// <summary>
        /// 业务名
        /// </summary>
        public string BusName { get; set; } = string.Empty;

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long? CreateUser { get; set; }

        /// <summary>
        /// 生成位置类型
        /// </summary>
        public string GenerateType { get; set; } = string.Empty;

        /// <summary>
        /// 包名称
        /// </summary>
        public string PackageName { get; set; } = string.Empty;

        /// <summary>
        /// 功能名
        /// </summary>
        public string TableComment { get; set; } = string.Empty;

        /// <summary>
        /// 数据库表名
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// 是否移除表前缀
        /// </summary>
        public string TablePrefix { get; set; } = string.Empty;

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
