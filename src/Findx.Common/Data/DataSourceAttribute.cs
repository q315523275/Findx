using System;

namespace Findx.Data
{
    /// <summary>
    /// 定义一个数据源配置属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DataSourceAttribute : Attribute
    {
        public DataSourceAttribute(string primary)
        {
            Primary = primary;
        }
        /// <summary>
        /// 数据源
        /// </summary>
        public string Primary { get; }
    }
}
