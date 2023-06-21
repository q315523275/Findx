using System.Text.Json.Serialization;

namespace Findx.Imaging;

/// <summary>
/// 合并图片Dto
/// </summary>
public class MergeImageDto
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="x">合并图片X轴坐标</param>
    /// <param name="y">合并图片Y轴坐标</param>
    /// <param name="width">合并图片宽度</param>
    /// <param name="height">合并图片高度</param>
    /// <param name="opacity">合并图片透明度</param>
    /// <param name="resizeMode">合并图片缩放方式</param>
    [JsonConstructor]
    public MergeImageDto(int x, int y, int width = 0, int height = 0, ImageResizeMode resizeMode = ImageResizeMode.None, float opacity = 0.75f)
    {
        X = x;
        Y = y;
        ResizeMode = resizeMode;
        Width = width;
        Height = height;
        Opacity = opacity;
    }

    /// <summary>
    /// 合并图片X轴坐标
    /// </summary>
    public int X { get; }
    
    /// <summary>
    /// 合并图片Y轴坐标
    /// </summary>
    public int Y { get; }
    
    /// <summary>
    /// 合并图片宽度
    /// </summary>
    public int Width  { get; } 
    
    /// <summary>
    /// 合并图片高度
    /// </summary>
    public int Height  { get; } 
    
    /// <summary>
    /// 合并图片透明度
    /// </summary>
    public float Opacity  { get; }

    /// <summary>
    /// 合并图片缩放方式
    /// </summary>
    public ImageResizeMode ResizeMode { get; }
}