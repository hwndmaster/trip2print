namespace TripToPrint.Core.Logging
{
    internal class GeneralLogger : LoggerBase
    {
        public GeneralLogger(ILogStorage logStorage) : base(logStorage)
        {
        }

        public override LogCategory Category => LogCategory.General;
    }
}
