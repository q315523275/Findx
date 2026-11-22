using Microsoft.Extensions.Logging;

namespace Findx.Jobs.Common;

/// <summary>
///     作业执行日志记录器：由执行宿主（如分布式调度组件）注入，Job 可通过
///     <see cref="JobExecuteContext.LogRecorder"/> 主动记录执行过程信息。
///     <para>与 ILogger 的关系是"补充"而不是替代：ILogger 面向运行日志，这里记录的是该次执行的过程轨迹，
///     会被宿主采集进执行记录（可落库、可在管理面按 TraceId 查看）。</para>
/// </summary>
public interface IJobLogRecorder
{
    /// <summary>
    ///     记录一条执行日志
    /// </summary>
    /// <param name="level">日志级别</param>
    /// <param name="message">日志内容</param>
    /// <param name="exception">关联异常（可空）</param>
    void Log(LogLevel level, string message, Exception exception = null);
}

/// <summary>
///     <see cref="IJobLogRecorder"/> 的便捷扩展（空实现安全）
/// </summary>
public static class JobLogRecorderExtensions
{
    /// <summary>
    ///     普通信息
    /// </summary>
    public static void Info(this IJobLogRecorder recorder, string message)
    {
        recorder?.Log(LogLevel.Information, message);
    }

    /// <summary>
    ///     警告
    /// </summary>
    public static void Warn(this IJobLogRecorder recorder, string message)
    {
        recorder?.Log(LogLevel.Warning, message);
    }

    /// <summary>
    ///     错误
    /// </summary>
    public static void Error(this IJobLogRecorder recorder, string message, Exception exception = null)
    {
        recorder?.Log(LogLevel.Error, message, exception);
    }
}
