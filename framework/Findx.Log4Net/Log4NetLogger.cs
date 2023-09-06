﻿using System;
using Findx.Common;
using log4net;
using Microsoft.Extensions.Logging;

namespace Findx.Log4Net
{
    public class Log4NetLogger : ILogger
    {
        private readonly ILog _log;

        public Log4NetLogger(string loggerRepository, string name)
        {
            _log = LogManager.GetLogger(loggerRepository, name);
        }

        public string Name => _log.Logger.Name;

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    return _log.IsDebugEnabled;
                case LogLevel.Information:
                    return _log.IsInfoEnabled;
                case LogLevel.Warning:
                    return _log.IsWarnEnabled;
                case LogLevel.Error:
                    return _log.IsErrorEnabled;
                case LogLevel.Critical:
                    return _log.IsFatalEnabled;
                case LogLevel.None:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            Check.NotNull(formatter, nameof(formatter));

            string message = null;
            if (formatter != null) message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
                switch (logLevel)
                {
                    case LogLevel.Trace:
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
                        _log.Error(message, exception);
                        break;
                    case LogLevel.Critical:
                        _log.Fatal(message, exception);
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