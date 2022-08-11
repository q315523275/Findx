using System;

namespace Findx.Data
{
    /// <summary>
    /// 定义一个数据源配置属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityExtensionAttribute : Attribute
    {
        /// <summary>
        /// 数据源
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// 表分片类型
        /// </summary>
        public ShardingType TableShardingType { get; set; } = ShardingType.None;

        /// <summary>
        /// 表分片扩展
        /// </summary>
        public string TableShardingExt { get; set; }
    }
}
