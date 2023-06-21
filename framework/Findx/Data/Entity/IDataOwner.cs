namespace Findx.Data;

/// <summary>
/// 数据拥有者接口
/// </summary>
public interface IDataOwner<TUser>: ICreatedTime where TUser : struct
{
    /// <summary>
    ///     拥有者用户
    /// </summary>
    TUser? OwnerId { get; set; }
}