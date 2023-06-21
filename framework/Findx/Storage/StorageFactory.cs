using Findx.DependencyInjection;

namespace Findx.Storage;

/// <summary>
///     文件存储服务工厂
/// </summary>
public class StorageFactory : ServiceFactoryBase<IFileStorage>, IStorageFactory
{
}