namespace TripToPrint.Core.Logging
{
    public interface IResourceFetchingLogger : ILogger
    {
    }

    internal class ResourceFetchingLogger : LoggerBase, IResourceFetchingLogger
    {
        public ResourceFetchingLogger(ILogStorage logStorage) : base(logStorage)
        {
        }

        public override LogCategory Category => LogCategory.ResourceFetching;
    }
}
