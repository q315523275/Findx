using Findx.DependencyInjection;

namespace Findx.Locks;

/// <summary>
/// 锁服务工厂
/// </summary>
public class LockFactory: ServiceFactoryBase<ILock>, ILockFactory;