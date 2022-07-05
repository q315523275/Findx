using System.ComponentModel;

namespace Findx.Data
{
    /// <summary>
    /// 分片类型
    /// </summary>
    public enum ShardingType
    {
        /// <summary>
        /// 不分片
        /// </summary>
        [Description("不分片")]
        None,
        /// <summary>
        /// 时间分片
        /// </summary>
        [Description("时间分片")]
        Time,
        /// <summary>
        /// 自定义分片
        /// </summary>
        [Description("自定义分片")]
        CustomSharding
    }
}
