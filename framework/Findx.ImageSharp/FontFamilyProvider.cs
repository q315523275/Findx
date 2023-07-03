using System.IO;
using SixLabors.Fonts;

namespace Findx.ImageSharp;

public class FontFamilyProvider: IFontFamilyProvider
{
    private static readonly FontCollection Collection = new();
    
    public FontFamily Create(string filePath)
    {
        return Collection.Add(filePath);
    }
    
    public FontFamily Create(Stream stream)
    {
        return Collection.Add(stream);
    }

    public bool TryGet(string fontFamily, out FontFamily outFontFamily)
    {
        return Collection.TryGet(fontFamily, out outFontFamily);
    }
}