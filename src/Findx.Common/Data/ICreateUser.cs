namespace Findx.Data
{
    /// <summary>
    /// 定义实体创建者
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public interface ICreateUser<TUser> : ICreateTime where TUser : struct
    {
        /// <summary>
        /// 创建者
        /// </summary>
        TUser? CreateUser {  get; set; }
    }
}
