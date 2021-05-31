using System;
using System.ComponentModel;

namespace Findx.Data
{
    /// <summary>
    /// 实体类基类
    /// </summary>
    public abstract class EntityBase : ValidatableObject
    {
    }
    /// <summary>
    /// 实体类基类
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class EntityBase<TKey> : EntityBase where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// 获取或设置 编号
        /// </summary>
        [DisplayName("编号")]
        public virtual TKey Id { get; set; }
    }
}
