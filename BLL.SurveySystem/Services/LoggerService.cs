using System;
using BLL.SurveySystem.Interfaces;
using NLog;

namespace BLL.SurveySystem.Services
{
    public class LoggerService<T> : ILoggerService<T>
    {
        public ILogger Logger { get; set; }
        public LoggerService()
        {
            Logger = LogManager.GetLogger(typeof(T).FullName);
        }
        public void Info(string message)
        {
            Logger.Info(message);
        }
        public void Warn(string message)
        {
            Logger.Warn(message);
        }
        public void Fatal(string message)
        {
            Logger.Fatal(message);
        }
        public void Error(string message)
        {
            Logger.Error(message);
        }
        public void Error(Exception exception)
        {
            Logger.Error(exception);
        }
        public void Fatal(Exception exception)
        {
            Logger.Fatal(exception);
        }
    }
}