namespace TripToPrint.Core.Logging
{
    public class GeneralLogger : LoggerBase
    {
        public GeneralLogger(ILogStorage logStorage) : base(logStorage)
        {
        }

        public override LogCategory Category => LogCategory.General;
    }
}
