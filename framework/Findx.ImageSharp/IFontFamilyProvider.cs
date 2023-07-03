using System.IO;
using SixLabors.Fonts;

namespace Findx.ImageSharp;

/// <summary>
/// 字体访问器
/// </summary>
public interface IFontFamilyProvider
{
    /// <summary>
    /// 创建字体
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    FontFamily Create(string filePath);

    /// <summary>
    /// 创建字体
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    FontFamily Create(Stream stream);

    /// <summary>
    /// 获取已存在字体
    /// </summary>
    /// <param name="fontFamily"></param>
    /// <param name="outFontFamily"></param>
    /// <returns></returns>
    bool TryGet(string fontFamily, out FontFamily outFontFamily);
}