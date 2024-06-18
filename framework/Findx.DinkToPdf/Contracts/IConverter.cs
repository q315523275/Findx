using System;
using Findx.DinkToPdf.EventDefinitions;

namespace Findx.DinkToPdf.Contracts;

/// <summary>
///     转换器
/// </summary>
public interface IConverter
{

    /// <summary>
    ///     基于给定设置转换文档
    /// </summary>
    /// <param name="document">要转换的文档</param>
    /// <returns>返回转换后的文档（以字节为单位）</returns>
    byte[] Convert(IDocument document);

    event EventHandler<PhaseChangedArgs> PhaseChanged;

    event EventHandler<ProgressChangedArgs> ProgressChanged;

    event EventHandler<FinishedArgs> Finished;

    event EventHandler<ErrorArgs> Error;

    event EventHandler<WarningArgs> Warning;

}