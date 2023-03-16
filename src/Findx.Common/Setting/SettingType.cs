using System.ComponentModel;

namespace Findx.Setting;

/// <summary>
/// 配置类型
/// </summary>
public enum SettingType
{
    /// <summary>
    /// 默认配置,读取IConfiguration对应配置
    /// </summary>
    [Description("IConfiguration实现")]
    Default = 0,
    
    /// <summary>
    /// 自定义数据库实现,业务应用自行实现
    /// </summary>
    [Description("自定义数据库实现")]
    CustomDatabase = 1,
}