namespace Findx.Data;

/// <summary>
/// 数据拥有者接口
/// </summary>
public interface IDataOwner<TUser> where TUser : struct
{
    /// <summary>
    ///     数据拥有者编号
    /// </summary>
    TUser? OwnerId { get; set; }
    
    /// <summary>
    ///     数据拥有者
    /// </summary>
    string Owner { get; set; }
}