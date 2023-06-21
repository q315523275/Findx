namespace Findx.Imaging;

/// <summary>
///     图片缩放方式
/// </summary>
public enum ImageResizeMode
{
    /// <summary>
    ///     none
    /// </summary>
    None = 0,
    
    /// <summary>
    /// 拉伸
    /// </summary>
    Stretch = 1,
    
    /// <summary>
    /// 填充
    /// </summary>
    BoxPad = 2,
    
    /// <summary>
    /// 以最小尺寸
    /// </summary>
    Min = 3,
    
    /// <summary>
    /// 以最大尺寸
    /// </summary>
    Max = 4,
    
    /// <summary>
    /// 裁剪
    /// </summary>
    Crop = 5,
    
    /// <summary>
    /// 以填充的方式,保持原图等比例
    /// </summary>
    Pad = 6,
    
    /// <summary>
    /// 默认
    /// </summary>
    Default = 7
}