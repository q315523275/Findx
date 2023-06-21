using Findx.Data;

namespace Findx.Imaging;

/// <summary>
/// 添加文本Dto
/// </summary>
public class DrawTextDto: ValidatableObject
{
    /// <summary>
    /// 添加文本起始X轴坐标
    /// </summary>
    public int X { set; get; } = 0;
    
    /// <summary>
    /// 添加文本起始Y轴坐标
    /// </summary>
    public int Y { set; get; } = 0;

    /// <summary>
    /// 行间距
    /// </summary>
    public float LineSpacing { set; get; } = 1f;
    
    /// <summary>
    /// 行长度
    /// </summary>
    public float WrappingLength { set; get; } = -1f;
    
    /// <summary>
    ///     字体名称
    /// </summary>
    public string FontFamily { set; get; }
    
    /// <summary>
    ///     字体文件路径
    /// </summary>
    public string FontFamilyPath { set; get; }

    /// <summary>
    ///     字体大写
    /// </summary>
    public int FontSize { set; get; } = 12;

    /// <summary>
    ///     字体颜色
    /// </summary>
    public string FontColor { set; get; } = "#000000";

    /// <summary>
    ///     是否加粗
    /// </summary>
    public FontStyle FontStyle { set; get; } = FontStyle.Regular;

    /// <summary>
    /// 是否使用遮罩
    /// </summary>
    public bool UseOverlay { set; get; } = false;
    
    /// <summary>
    /// 遮罩颜色
    /// </summary>
    public string OverlayColor { set; get; } = "#000000";
    
    /// <summary>
    /// 遮罩透明度
    /// </summary>
    public float OverlayOpacity { set; get; } = 0.7f;
    
    /// <summary>
    /// 遮罩左边距
    /// </summary>
    public int OverlayX { set; get; } = 10;
    
    /// <summary>
    /// 遮罩上边距
    /// </summary>
    public int OverlayY { set; get; } = 10;
}