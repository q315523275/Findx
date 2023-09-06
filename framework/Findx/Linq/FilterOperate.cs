using System.ComponentModel;

namespace Findx.Linq;

/// <summary>
///     筛选操作
/// </summary>
public enum FilterOperate
{
    /// <summary>
    ///     并且
    /// </summary>
    [Description("并且")]
    And = 1,

    /// <summary>
    ///     或者
    /// </summary>
    [Description("或者")]
    Or = 2,

    /// <summary>
    ///     等于
    /// </summary>
    [Description("等于")]
    Equal = 3,

    /// <summary>
    ///     不等于
    /// </summary>
    [Description("不等于")]
    NotEqual = 4,

    /// <summary>
    ///     小于
    /// </summary>
    [Description("小于")]
    Less = 5,

    /// <summary>
    ///     小于或等于
    /// </summary>
    [Description("小于等于")]
    LessOrEqual = 6,

    /// <summary>
    ///     大于
    /// </summary>
    [Description("大于")]
    Greater = 7,

    /// <summary>
    ///     大于或等于
    /// </summary>
    [Description("大于等于")]
    GreaterOrEqual = 8,

    /// <summary>
    ///     以……开始
    /// </summary>
    [Description("开始于")]
    StartsWith = 9,

    /// <summary>
    ///     以……结束
    /// </summary>
    [Description("结束于")]
    EndsWith = 10,

    /// <summary>
    ///     字符串的包含（相似）
    /// </summary>
    [Description("包含")]
    Contains = 11,

    /// <summary>
    ///     字符串的不包含
    /// </summary>
    [Description("不包含")]
    NotContains = 12,
    
    /// <summary>
    ///     包括在
    /// </summary>
    [Description("包括在")]
    In = 13,

    /// <summary>
    ///     不包括在
    /// </summary>
    [Description("不包括在")]
    NotIn = 14,
    
    /// <summary>
    ///     范围
    /// </summary>
    [Description("范围")]
    Between = 15
}