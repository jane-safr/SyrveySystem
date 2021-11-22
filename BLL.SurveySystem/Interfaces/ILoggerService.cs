using System;

namespace BLL.SurveySystem.Interfaces
{
    public interface ILoggerService<T>
    {
        void Info(string message);
        void Warn(string message);
        void Fatal(string message);
        void Error(string message);
        void Error(Exception exception);
        void Fatal(Exception exception);
    }
}