using System.Collections.Generic;

namespace Findx.DinkToPdf.Contracts;

public interface IDocument : ISettings
{
    IEnumerable<IObject> GetObjects();
}