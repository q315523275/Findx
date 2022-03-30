using System;

namespace Findx.Data
{
    /// <summary>
    /// 定义实体逻辑删除
    /// </summary>
    public interface ISoftDeletable
    {
        /// <summary>
        /// 逻辑删除时间
        /// </summary>
        DateTime? DeletionTime { get; set; }

        /// <summary>
        /// 是否逻辑删除
        /// </summary>
        bool IsDeleted { get; set; }
    }
}
