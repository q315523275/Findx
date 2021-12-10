using System.ComponentModel;

namespace Findx.Data
{
    /// <summary>
    /// 实体类基类
    /// </summary>
    public interface IEntity { }

    /// <summary>
    /// 实体类基类
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntity<TKey> : IEntity where TKey : struct
    {
        /// <summary>
        /// 获取或设置 编号
        /// </summary>
        TKey Id { get; set; }
    }


    /// <summary>
    /// 实体类基类
    /// </summary>
    public abstract class EntityBase : ValidatableObject, IEntity
    {
        /// <summary>
        /// 实体初始化
        /// </summary>
        public virtual void Init() { }
    }

    /// <summary>
    /// 实体类基类
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class EntityBase<TKey> : EntityBase, IEntity<TKey> where TKey : struct
    {
        /// <summary>
        /// 获取或设置 编号
        /// </summary>
        [DisplayName("编号")]
        public abstract TKey Id { get; set; }
    }
}
