namespace Findx.Data
{
    /// <summary>
    /// 定义实体分表
    /// </summary>
    public interface ITableSharding
    {
        /// <summary>
        /// 查询分片表名
        /// </summary>
        /// <returns></returns>
        string GetShardingName(IEntity entity);
    }
}
