/// <summary>
///     进程执行异常信息
/// </summary>
// ReSharper disable once CheckNamespace
public class ProcessErrorException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="exitCode"></param>
    /// <param name="errorOutput"></param>
    public ProcessErrorException(int exitCode, string[] errorOutput) : base("Process returns error, ExitCode:" + exitCode + Environment.NewLine + string.Join(Environment.NewLine, errorOutput))
    {
        ExitCode = exitCode;
        ErrorOutput = errorOutput;
    }

    /// <summary>
    ///     退出编号
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    ///     异常消息集合
    /// </summary>
    public string[] ErrorOutput { get; }
}