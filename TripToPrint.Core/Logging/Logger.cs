﻿namespace TripToPrint.Core.Logging
{
    public interface ILogger
    {
        void Info(string text);
        void Warn(string text);
        void Error(string text);
    }

    public class Logger : ILogger
    {
        private readonly ILogStorage _logStorage;

        public Logger(ILogStorage logStorage)
        {
            _logStorage = logStorage;
        }

        public void Info(string text)
        {
            _logStorage.WriteLog(new LogItem(LogSeverity.Info, text));
        }

        public void Warn(string text)
        {
            _logStorage.WriteLog(new LogItem(LogSeverity.Warning, text));
        }

        public void Error(string text)
        {
            _logStorage.WriteLog(new LogItem(LogSeverity.Error, text));
        }
    }
}