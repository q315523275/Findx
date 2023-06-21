using System.ComponentModel;

namespace Findx.Imaging;

/// <summary>
///     字体样式
/// </summary>
public enum FontStyle
{
    /// <summary>
    ///     正常
    /// </summary>
    [Description("正常")] 
    Regular,

    /// <summary>
    ///     加粗
    /// </summary>
    [Description("加粗")] 
    Bold,

    /// <summary>
    ///     斜体
    /// </summary>
    [Description("斜体")] 
    Italic,

    /// <summary>
    ///     加粗斜体
    /// </summary>
    [Description("加粗斜体")] 
    BoldItalic
}