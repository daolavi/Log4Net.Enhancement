using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;

namespace Log4Net.Enhancement.Example
{
    class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            var logRepo = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepo, new FileInfo("log4net.config"));

            _logger.Debug("Debug message");
            _logger.Info("Info message");
            _logger.Warn("Warning message");
            try
            {
                var a = 0;
                var b = 1 / a;
            }
            catch (Exception ex)
            {
                _logger.Error("Error occurs", ex);
            }
            _logger.Fatal("Fatal error");

            LogManager.Shutdown();
        }
    }
}
