using System;

namespace TripToPrint.Core
{
    public interface IProgressTrackerFactory
    {
        IProgressTracker Create(Action<int> handler);
    }

    public class ProgressTrackerFactory : IProgressTrackerFactory
    {
        public IProgressTracker Create(Action<int> handler)
        {
            return new ProgressTracker(new Progress<int>(handler));
        }
    }
}
