using Findx.DependencyInjection;

namespace Findx.Locks;

/// <summary>
/// 锁服务工厂
/// </summary>
public interface ILockFactory: IServiceFactory<ILock>
{
}