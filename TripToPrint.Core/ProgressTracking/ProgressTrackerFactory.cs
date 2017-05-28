using System;

namespace TripToPrint.Core.ProgressTracking
{
    public interface IProgressTrackerFactory
    {
        IDiscoveringProgress CreateForDiscovering(Action<int> handler);
        IResourceFetchingProgress CreateForResourceFetching(Action<int> handler);
    }

    public class ProgressTrackerFactory : IProgressTrackerFactory
    {
        public IDiscoveringProgress CreateForDiscovering(Action<int> handler)
        {
            return new DiscoveringProgress(new Progress<int>(handler));
        }

        public IResourceFetchingProgress CreateForResourceFetching(Action<int> handler)
        {
            return new ResourceFetchingProgress(new Progress<int>(handler));
        }
    }
}
