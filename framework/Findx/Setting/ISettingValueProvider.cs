namespace Findx.Setting;

/// <summary>
///     设置值提供器接口
/// </summary>
public interface ISettingValueProvider
{
    /// <summary>
    ///     提供器名称
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     从大到小开始获取
    /// </summary>
    int Order { get; }

    /// <summary>
    ///     获取值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    T GetValue<T>(string key);

    /// <summary>
    ///     获取对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    T GetObject<T>(string key) where T : new();
}