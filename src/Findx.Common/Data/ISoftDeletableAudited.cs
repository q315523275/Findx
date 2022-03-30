namespace Findx.Data
{
    /// <summary>
    /// 定义实体删除者
    /// </summary>
    public interface ISoftDeletableUser<TUserKey> : ISoftDeletable where TUserKey : struct
    {
        /// <summary>
        /// 逻辑删除操作人
        /// </summary>
        TUserKey? DeleterId { get; set; }
    }
}
