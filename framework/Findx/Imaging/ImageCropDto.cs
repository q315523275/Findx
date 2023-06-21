using System.Text.Json.Serialization;
using Findx.Data;

namespace Findx.Imaging;

/// <summary>
/// 图片裁剪Dto
/// </summary>
public class ImageCropDto: ValidatableObject
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="x">矩形区域X坐标</param>
    /// <param name="y">矩形区域Y坐标s</param>
    /// <param name="width">矩形区域长宽度</param>
    /// <param name="height">矩形区域长高度</param>
    [JsonConstructor]
    public ImageCropDto(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// 矩形区域X坐标
    /// </summary>
    public int X  { get; } 
    
    /// <summary>
    /// 矩形区域Y坐标
    /// </summary>
    public int Y  { get; } 
    
    /// <summary>
    /// 矩形区域长宽度
    /// </summary>
    public int Width  { get; } 
    
    /// <summary>
    /// 矩形区域长高度
    /// </summary>
    public int Height  { get; } 
}