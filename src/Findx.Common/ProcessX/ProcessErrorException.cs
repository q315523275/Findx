/// <summary>
/// 进程执行异常信息
/// </summary>
public class ProcessErrorException : Exception
{
    /// <summary>
    /// 退出编号
    /// </summary>
    public int ExitCode { get; }
    
    /// <summary>
    /// 异常消息集合
    /// </summary>
    public string[] ErrorOutput { get; }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="exitCode"></param>
    /// <param name="errorOutput"></param>
    public ProcessErrorException(int exitCode, string[] errorOutput) : base("Process returns error, ExitCode:" + exitCode + Environment.NewLine + string.Join(Environment.NewLine, errorOutput))
    {
        ExitCode = exitCode;
        ErrorOutput = errorOutput;
    }
}