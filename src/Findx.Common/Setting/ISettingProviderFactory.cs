namespace Findx.Setting;

/// <summary>
/// 配置提供器工厂
/// </summary>
public interface ISettingProviderFactory
{
    /// <summary>
    /// 获取配置提供器
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    ISettingProvider Get(SettingType type = SettingType.Default);
}