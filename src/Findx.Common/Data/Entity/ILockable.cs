namespace Findx.Data;

/// <summary>
///     约定实体是否锁定
/// </summary>
public interface ILockable
{
    /// <summary>
    ///     获取或设置 是否锁定当前信息
    /// </summary>
    bool IsLocked { get; set; }
}