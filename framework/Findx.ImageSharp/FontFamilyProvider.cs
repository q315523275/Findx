using System.Collections.Concurrent;
using System.IO;
using SixLabors.Fonts;

namespace Findx.ImageSharp;

public class FontFamilyProvider: IFontFamilyProvider
{
    private static readonly FontCollection Collection = new();

    /// <summary>
    ///     按文件路径缓存已加载字体，避免同一路径重复Add造成静态集合无限膨胀
    /// </summary>
    private static readonly ConcurrentDictionary<string, FontFamily> FileCache = new();

    /// <summary>
    ///     SixLabors.Fonts(2.x) 的 FontCollection 未承诺线程安全，对Collection的读写统一加锁串行化
    /// </summary>
    private static readonly object SyncRoot = new();

    public FontFamily Create(string filePath)
    {
        return FileCache.GetOrAdd(filePath, path =>
        {
            lock (SyncRoot)
            {
                return Collection.Add(path);
            }
        });
    }

    public FontFamily Create(Stream stream)
    {
        lock (SyncRoot)
        {
            return Collection.Add(stream);
        }
    }

    public bool TryGet(string fontFamily, out FontFamily outFontFamily)
    {
        lock (SyncRoot)
        {
            return Collection.TryGet(fontFamily, out outFontFamily);
        }
    }
}
