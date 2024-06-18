using System;
using Findx.DinkToPdf.Contracts;

namespace Findx.DinkToPdf.EventDefinitions;

public class FinishedArgs : EventArgs
{
    public IDocument Document { get; set; }

    public bool Success { get; set; }
}