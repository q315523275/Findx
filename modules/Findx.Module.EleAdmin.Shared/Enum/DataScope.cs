using System.ComponentModel;

namespace Findx.Module.EleAdmin.Shared.Enum;

/// <summary>
///     数据范围
/// </summary>
public enum DataScope
{
    /// <summary>
    ///     全部数据
    /// </summary>
    [Description("全部数据")]
    All = 1,
    
    /// <summary>
    ///     自定义数据
    /// </summary>
    [Description("自定义数据")]
    Custom = 2,
    
    /// <summary>
    ///     本部门
    /// </summary>
    [Description("本部门")]
    Department = 3,
    
    /// <summary>
    ///     本部门及以下数据权限
    /// </summary>
    [Description("本部门及以下数据权限")]
    Subs = 4,
    
    /// <summary>
    ///     本人数据权限
    /// </summary>
    [Description("本人数据权限")]
    Oneself = 5
}