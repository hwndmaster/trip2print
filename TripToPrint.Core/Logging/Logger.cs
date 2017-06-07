using System;

namespace TripToPrint.Core.Logging
{
    public interface ILogger
    {
        LogCategory Category { get; }

        void Info(string text);
        void Warn(string text);
        void Error(string text);
        void Exception(Exception exception);
    }

    internal abstract class LoggerBase : ILogger
    {
        private readonly ILogStorage _logStorage;

        protected LoggerBase(ILogStorage logStorage)
        {
            _logStorage = logStorage;
        }

        public abstract LogCategory Category { get; }

        public void Info(string text)
        {
            _logStorage.WriteLog(new LogItem(Category, LogSeverity.Info, text));
        }

        public void Warn(string text)
        {
            _logStorage.WriteLog(new LogItem(Category, LogSeverity.Warning, text));
        }

        public void Error(string text)
        {
            _logStorage.WriteLog(new LogItem(Category, LogSeverity.Error, text));
        }

        public void Exception(Exception exception)
        {
            _logStorage.WriteLog(new LogItem(Category, LogSeverity.Fatal, exception.Message));
        }
    }
}
