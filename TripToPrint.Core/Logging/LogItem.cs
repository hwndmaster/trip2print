namespace TripToPrint.Core.Logging
{
    public class LogItem
    {
        public LogItem(LogSeverity severity, string text)
        {
            this.Severity = severity;
            this.Text = text;
        }

        public LogSeverity Severity { get; set; }
        public string Text { get; set; }
    }
}
