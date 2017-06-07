namespace TripToPrint.Core.Logging
{
    public class LogItem
    {
        public LogItem(LogCategory category, LogSeverity severity, string text)
        {
            this.Category = category;
            this.Severity = severity;
            this.Text = text;
        }

        public LogCategory Category { get; set; }
        public LogSeverity Severity { get; set; }
        public string Text { get; set; }

        public bool IsWarningOrError => Severity == LogSeverity.Warning
                                        || Severity == LogSeverity.Error
                                        || Severity == LogSeverity.Fatal;
    }
}
