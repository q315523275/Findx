using System;
using Findx.Common;
using Microsoft.Extensions.Logging;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Findx.NLog
{
    public class NLogLogger : ILogger
    {
        private readonly Logger _log;

        public NLogLogger(string name)
        {
            _log = LogManager.GetLogger(name);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return _log.IsFatalEnabled;
                case LogLevel.Debug:
                    return _log.IsDebugEnabled;
                case LogLevel.Trace:
                    return _log.IsTraceEnabled;
                case LogLevel.Error:
                    return _log.IsErrorEnabled;
                case LogLevel.Information:
                    return _log.IsInfoEnabled;
                case LogLevel.Warning:
                    return _log.IsWarnEnabled;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            Check.NotNull(formatter, nameof(formatter));

            string message = null;
            if (null != formatter) message = formatter(state, exception);
            if (!string.IsNullOrEmpty(message) || exception != null)
                switch (logLevel)
                {
                    case LogLevel.Trace:
                        _log.Trace(message);
                        break;
                    case LogLevel.Debug:
                        _log.Debug(message);
                        break;
                    case LogLevel.Information:
                        _log.Info(message);
                        break;
                    case LogLevel.Warning:
                        _log.Warn(message);
                        break;
                    case LogLevel.Error:
                        _log.Error(exception, message);
                        break;
                    case LogLevel.Critical:
                        _log.Fatal(exception, message);
                        break;
                    case LogLevel.None:
                        break;
                    default:
                        _log.Warn($"遇到未知的日志级别 {logLevel}, 使用Info级别写入日志。");
                        _log.Info(message, exception);
                        break;
                }
        }
    }
}