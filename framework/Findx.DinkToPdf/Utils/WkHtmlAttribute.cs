using System;

namespace Findx.DinkToPdf.Utils;

[AttributeUsage(AttributeTargets.Property)]
public class WkHtmlAttribute : Attribute
{
    public string Name { get; private set; }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="name"></param>
    public WkHtmlAttribute(string name)
    {
        Name = name;
    }
}