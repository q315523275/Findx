using System.Collections.Generic;
using Findx.DinkToPdf.Contracts;
using Findx.DinkToPdf.Settings;

namespace Findx.DinkToPdf.Documents;

public class HtmlToPdfDocument : IDocument
{
    public List<ObjectSettings> Objects { get; }

    /// <summary>
    ///     全局配置
    /// </summary>
    public GlobalSettings GlobalSettings { get; set; } = new();

    /// <summary>
    ///     Ctor
    /// </summary>
    public HtmlToPdfDocument()
    {
        Objects = [];
    }

    public IEnumerable<IObject> GetObjects()
    {
        return Objects.ToArray();
    }
}