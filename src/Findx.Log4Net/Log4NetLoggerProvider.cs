using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Findx.Extensions;
using log4net;
using log4net.Config;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.Logging;

namespace Findx.Log4Net
{
    public class Log4NetLoggerProvider : ILoggerProvider
    {
        private const string DefaultLog4NetFileName = "log4net.config";
        private readonly ILoggerRepository _loggerRepository;

        private readonly IDictionary<string, Log4NetLogger>
            _loggers = new ConcurrentDictionary<string, Log4NetLogger>();

        public Log4NetLoggerProvider(string log4NetConfigFile)
        {
            var file = log4NetConfigFile ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultLog4NetFileName);
            if (!File.Exists(file)) throw new FileNotFoundException("未发现“log4net.config”位置文件");
            var assembly = Assembly.GetEntryAssembly() ?? GetCallingAssemblyFromStartup();
            _loggerRepository = LogManager.CreateRepository(assembly, typeof(Hierarchy));
            XmlConfigurator.ConfigureAndWatch(_loggerRepository, new FileInfo(file));
        }

        public Log4NetLoggerProvider() : this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            DefaultLog4NetFileName))
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, key => new Log4NetLogger(_loggerRepository.Name, key));
        }

        public void Dispose()
        {
            _loggers?.Clear();
        }

        private static Assembly GetCallingAssemblyFromStartup()
        {
            var stackTrace = new StackTrace(2);
            for (var i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var type = frame.GetMethod()?.DeclaringType;

                if (string.Equals(type?.Name, "Startup", StringComparison.OrdinalIgnoreCase)) return type?.Assembly;
            }

            return null;
        }
    }
}