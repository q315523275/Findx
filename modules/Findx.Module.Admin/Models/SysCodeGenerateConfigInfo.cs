using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 代码生成详细配置
    /// </summary>
    public partial class SysCodeGenerateConfig
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 代码生成主表ID
        /// </summary>
        public long? CodeGenId { get; set; }

        /// <summary>
        /// 字段描述
        /// </summary>
        public string ColumnComment { get; set; } = string.Empty;

        /// <summary>
        /// 主键
        /// </summary>
        public string ColumnKey { get; set; } = string.Empty;

        /// <summary>
        /// 主外键名称
        /// </summary>
        public string ColumnKeyName { get; set; } = string.Empty;

        /// <summary>
        /// 数据库字段名
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long? CreateUser { get; set; }

        /// <summary>
        /// 物理类型
        /// </summary>
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// 字典code
        /// </summary>
        public string DictTypeCode { get; set; } = string.Empty;

        /// <summary>
        /// 作用类型（字典）
        /// </summary>
        public string EffectType { get; set; } = string.Empty;

        /// <summary>
        /// java类字段名
        /// </summary>
        public string JavaName { get; set; } = string.Empty;

        /// <summary>
        /// java类型
        /// </summary>
        public string JavaType { get; set; } = string.Empty;

        /// <summary>
        /// 查询方式
        /// </summary>
        public string QueryType { get; set; } = string.Empty;

        /// <summary>
        /// 是否是查询条件
        /// </summary>
        public string QueryWhether { get; set; } = string.Empty;

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public long? UpdateUser { get; set; }

        /// <summary>
        /// 增改
        /// </summary>
        public string WhetherAddUpdate { get; set; } = string.Empty;

        /// <summary>
        /// 是否是通用字段
        /// </summary>
        public string WhetherCommon { get; set; } = string.Empty;

        /// <summary>
        /// 是否必填（字典）
        /// </summary>
        public string WhetherRequired { get; set; } = string.Empty;

        /// <summary>
        /// 列表是否缩进（字典）
        /// </summary>
        public string WhetherRetract { get; set; } = string.Empty;

        /// <summary>
        /// 列表展示
        /// </summary>
        public string WhetherTable { get; set; } = string.Empty;

    }

}
