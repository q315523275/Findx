﻿using Findx.Extensions;

namespace Findx.Logging;

/// <summary>
///     启动记录器
/// </summary>
public class StartupLogger
{
    /// <summary>
    ///     日志信息集合
    /// </summary>
    public IList<LogInfo> LogInfos { get; } = new List<LogInfo>();

    /// <summary>
    ///     Information日志
    /// </summary>
    /// <param name="message"></param>
    /// <param name="logName"></param>
    public void LogInformation(string message, string logName)
    {
        Log(LogLevel.Information, message, logName);
    }

    /// <summary>
    ///     Debug日志
    /// </summary>
    /// <param name="message"></param>
    /// <param name="logName"></param>
    public void LogDebug(string message, string logName)
    {
        Log(LogLevel.Debug, message, logName);
    }

    /// <summary>
    ///     记录日志
    /// </summary>
    /// <param name="logLevel"></param>
    /// <param name="message"></param>
    /// <param name="logName"></param>
    /// <param name="exception"></param>
    public void Log(LogLevel logLevel, string message, string logName, Exception exception = null)
    {
        var log = new LogInfo
        {
            LogLevel = logLevel, Message = message, LogName = logName, Exception = exception, CreatedTime = DateTime.Now
        };
        LogInfos.Add(log);
    }

    /// <summary>
    ///     打印日志
    /// </summary>
    /// <param name="provider"></param>
    public void Print(IServiceProvider provider)
    {
        IDictionary<string, ILogger> dict = new Dictionary<string, ILogger>();
        foreach (var info in LogInfos.OrderBy(m => m.CreatedTime))
        {
            if (info.Message.IsNullOrWhiteSpace())
                continue;

            if (!dict.TryGetValue(info.LogName, out var logger))
            {
                logger = provider.GetLogger(info.LogName);
                dict[info.LogName] = logger;
            }

            switch (info.LogLevel)
            {
                case LogLevel.Trace:
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    logger.LogTrace(info.Message);
                    break;
                case LogLevel.Debug:
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    logger.LogDebug(info.Message);
                    break;
                case LogLevel.Information:
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    logger.LogInformation(info.Message);
                    break;
                case LogLevel.Warning:
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    logger.LogWarning(info.Message);
                    break;
                case LogLevel.Error:
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    logger.LogError(info.Exception, info.Message);
                    break;
                case LogLevel.Critical:
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    logger.LogCritical(info.Exception, info.Message);
                    break;
            }
        }
    }

    /// <summary>
    ///     日志信息
    /// </summary>
    public class LogInfo
    {
        /// <summary>
        ///     等级
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        ///     消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     异常信息
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        ///     日志名
        /// </summary>
        public string LogName { get; set; }

        /// <summary>
        ///     创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }
}