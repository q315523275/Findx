using System;
using Findx.DinkToPdf.Contracts;

namespace Findx.DinkToPdf.EventDefinitions;

public class ErrorArgs : EventArgs
{
    public IDocument Document { get; set; }

    public string Message { get; set; }
}