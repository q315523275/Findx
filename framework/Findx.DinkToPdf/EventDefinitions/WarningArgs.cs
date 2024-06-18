using System;
using Findx.DinkToPdf.Contracts;

namespace Findx.DinkToPdf.EventDefinitions;

public class WarningArgs : EventArgs
{
    public IDocument Document { get; set; }

    public string Message { get; set; }
}