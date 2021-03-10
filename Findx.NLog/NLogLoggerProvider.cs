using NLog;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace Findx.NLog
{
    public class NLogLoggerProvider : Microsoft.Extensions.Logging.ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, NLogLogger> _loggers = new ConcurrentDictionary<string, NLogLogger>();
        private const string DefaultNLogFileName = "nlog.config";

        public NLogLoggerProvider(string nlogConfigFile)
        {
            string file = nlogConfigFile ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultNLogFileName);
            if (!File.Exists(file)) throw new FileNotFoundException("未发现“nlog.config”位置文件");
            LogManager.LoadConfiguration(file);
        }

        public NLogLoggerProvider() : this(DefaultNLogFileName) { }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, key => new NLogLogger(key));
        }

        public void Dispose()
        {
            _loggers?.Clear();
        }
    }
}
