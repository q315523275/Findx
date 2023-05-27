using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Findx.Extensions;
using Microsoft.Extensions.Logging;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Findx.NLog
{
    public class NLogLoggerProvider : ILoggerProvider
    {
        private const string DefaultNLogFileName = "nlog.config";
        private readonly IDictionary<string, NLogLogger> _loggers = new ConcurrentDictionary<string, NLogLogger>();

        public NLogLoggerProvider(string nlogConfigFile)
        {
            var file = nlogConfigFile ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultNLogFileName);
            if (!File.Exists(file)) throw new FileNotFoundException("未发现“nlog.config”位置文件");
            LogManager.LoadConfiguration(file);
        }

        public NLogLoggerProvider() : this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultNLogFileName))
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, key => new NLogLogger(key));
        }

        public void Dispose()
        {
            _loggers?.Clear();
        }
    }
}