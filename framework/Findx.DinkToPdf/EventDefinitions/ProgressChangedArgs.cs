using System;
using Findx.DinkToPdf.Contracts;

namespace Findx.DinkToPdf.EventDefinitions;

public class ProgressChangedArgs : EventArgs
{
    public IDocument Document { get; set; }

    public string Description { get; set; }
}