namespace Findx.Locks;

/// <summary>
///     锁服务提供器
/// </summary>
public class LockProvider : ILockProvider
{
    private readonly IDictionary<LockType, ILock> _locks;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="locks"></param>
    public LockProvider(IEnumerable<ILock> locks)
    {
        _locks = locks.ToDictionary(it => it.LockType, it => it);
        ;
    }

    /// <summary>
    ///     获取锁服务
    /// </summary>
    /// <param name="lockType"></param>
    /// <returns></returns>
    public ILock Get(LockType lockType)
    {
        _locks.TryGetValue(lockType, out var _lock);

        Check.NotNull(_lock, nameof(_lock));

        return _lock;
    }
}