using SixLabors.Fonts;

namespace Findx.ImageSharp;

public class FontFamilyAccessor: IFontFamilyAccessor
{
    private readonly FontCollection _collection = new();
}