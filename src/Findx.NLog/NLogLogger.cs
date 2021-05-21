using Microsoft.Extensions.Logging;
using NLog;
using System;

namespace Findx.NLog
{
    public class NLogLogger : Microsoft.Extensions.Logging.ILogger
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

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    return _log.IsFatalEnabled;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    return _log.IsDebugEnabled;
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    return _log.IsTraceEnabled;
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    return _log.IsErrorEnabled;
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    return _log.IsInfoEnabled;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    return _log.IsWarnEnabled;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            Check.NotNull(formatter, nameof(formatter));

            string message = null;
            if (null != formatter)
            {
                message = formatter(state, exception);
            }
            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                switch (logLevel)
                {
                    case Microsoft.Extensions.Logging.LogLevel.Critical:
                        _log.Fatal(message);
                        break;
                    case Microsoft.Extensions.Logging.LogLevel.Debug:
                        _log.Debug(message);
                        break;
                    case Microsoft.Extensions.Logging.LogLevel.Trace:
                        _log.Trace(message);
                        break;
                    case Microsoft.Extensions.Logging.LogLevel.Error:
                        _log.Error(message, exception, null);
                        break;
                    case Microsoft.Extensions.Logging.LogLevel.Information:
                        _log.Info(message);
                        break;
                    case Microsoft.Extensions.Logging.LogLevel.Warning:
                        _log.Warn(message);
                        break;
                    default:
                        _log.Warn($"遇到未知的日志级别 {logLevel}, 使用Info级别写入日志。");
                        _log.Info(message, exception, null);
                        break;
                }
            }
        }
    }
}
