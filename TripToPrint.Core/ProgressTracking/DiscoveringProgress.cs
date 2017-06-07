using System;

namespace TripToPrint.Core.ProgressTracking
{
    public interface IDiscoveringProgress
    {
        void ReportNumberOfIterations(int count);
        void ReportItemProcessed();
        void ReportDone();
    }

    internal class DiscoveringProgress : IDiscoveringProgress
    {
        private readonly IProgress<int> _progress;

        private int _venueCount;
        private int _venuesProcessed;
        private bool _done;

        public DiscoveringProgress(IProgress<int> progress)
        {
            _progress = progress;
        }

        public void ReportNumberOfIterations(int count)
        {
            _venueCount = count;
            _progress.Report(CalculateValue());
        }

        public void ReportItemProcessed()
        {
            _venuesProcessed++;
            _progress.Report(CalculateValue());
        }

        public void ReportDone()
        {
            _done = true;
            _progress.Report(CalculateValue());
        }

        private int CalculateValue()
        {
            if (_done)
            {
                return 100;
            }

            var sum = 0;

            if (_venuesProcessed > 0)
            {
                sum += (int)((float)_venuesProcessed / _venueCount * 99);
            }

            return sum;
        }
    }
}
