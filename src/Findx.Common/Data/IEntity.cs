namespace Findx.Data
{
    public interface IEntity { }
    /// <summary>
    /// 数据模型接口
    /// </summary>
    public interface IEntity<TKey>
    {
        /// <summary>
        /// 获取或设置 实体唯一标识，主键
        /// </summary>
        TKey Id { get; set; }
    }
}

