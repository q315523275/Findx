using System;

namespace Findx.Data
{
    /// <summary>
    /// 定义实体创建时间
    /// </summary>
    public interface ICreateTime
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime CreateTime { get; set; }
    }
}
