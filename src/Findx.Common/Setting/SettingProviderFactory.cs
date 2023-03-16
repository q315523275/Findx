namespace Findx.Setting;

/// <summary>
/// 配置提供器工厂
/// </summary>
public class SettingProviderFactory: ISettingProviderFactory
{
    private readonly IDictionary<SettingType, ISettingProvider> _settingProviders;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="settingProviders"></param>
    public SettingProviderFactory(IEnumerable<ISettingProvider> settingProviders)
    {
        _settingProviders = settingProviders.ToDictionary(x => x.Type, x => x);
    }

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public ISettingProvider Get(SettingType type = SettingType.Default)
    {
        if (_settingProviders.TryGetValue(type, out var settingProvider))
        {
            return settingProvider;
        }

        throw new KeyNotFoundException($"The {type.ToString()} ISettingProvider Not Found!");
    }
}