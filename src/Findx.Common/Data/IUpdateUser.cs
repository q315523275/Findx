using System;

namespace Findx.Data
{
    /// <summary>
    /// 定义实体更新用户字段
    /// </summary>
    public interface IUpdateUser<TUser> where TUser : struct
    {
        /// <summary>
        /// 更新时间
        /// </summary>
        DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新用户
        /// </summary>
        TUser? UpdateUser {  get; set; }
    }
}
