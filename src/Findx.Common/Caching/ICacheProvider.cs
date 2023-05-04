namespace Findx.Caching;

/// <summary>
///     缓存提供器
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    ///     获取缓存实例
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    ICache Get(string name = null);
}