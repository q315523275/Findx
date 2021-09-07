namespace Findx.Data
{
    /// <summary>
    /// 定义实体创建者
    /// </summary>
    /// <typeparam name="TUserKey"></typeparam>
    public interface ICreateUser<TUserKey> : ICreateTime where TUserKey : struct
    {
        /// <summary>
        /// 创建者
        /// </summary>
        TUserKey? CreateUser { get; set; }
    }
}
