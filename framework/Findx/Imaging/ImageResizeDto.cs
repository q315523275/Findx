using System.Text.Json.Serialization;
using Findx.Data;

namespace Findx.Imaging;

/// <summary>
/// 图片缩放Dto
/// </summary>
public class ImageResizeDto: ValidatableObject
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="mode"></param>
    [JsonConstructor]
    public ImageResizeDto(int width, int height, ImageResizeMode? mode = null)
    {
        Width = width;
        Height = height;
        if (mode.HasValue)
        {
            Mode = mode.Value;
        }
    }

    /// <summary>
    /// 缩放模式
    /// </summary>
    public ImageResizeMode Mode { get; } = ImageResizeMode.Default;
    
    /// <summary>
    /// 宽度
    /// </summary>
    public int Width  { get; } 
    
    /// <summary>
    /// 高度
    /// </summary>
    public int Height  { get; } 
}