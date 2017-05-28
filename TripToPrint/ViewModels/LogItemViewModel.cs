using System.Windows;
using System.Windows.Media;

using TripToPrint.Core.Logging;

namespace TripToPrint.ViewModels
{
    public class LogItemViewModel : LogItem
    {
        public LogItemViewModel(LogItem logItem) : base(logItem.Category, logItem.Severity, logItem.Text)
        {
        }

        public string ImagePath => $"/Resources/{Severity}.gif";
        public Color ItemColor => GetColorByLogSeverity(Severity);

        private Color GetColorByLogSeverity(LogSeverity logSeverity)
        {
            switch (logSeverity)
            {
                case LogSeverity.Warning:
                    return Colors.Chocolate;
                case LogSeverity.Error:
                    return Colors.OrangeRed;
                case LogSeverity.Fatal:
                    return Colors.DarkRed;
                case LogSeverity.Info:
                default:
                    return SystemColors.WindowTextColor;
            }
        }
    }
}
