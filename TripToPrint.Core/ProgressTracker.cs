using System;
using System.Collections.Generic;

namespace TripToPrint.Core
{
    public interface IProgressTracker
    {
        void ReportResourceEntriesProcessed();
        void ReportContentGenerationDone();
        void ReportDone();
        void ReportFetchImagesCount(int count);
        void ReportFetchImageProcessed();
    }

    public class ProgressTracker : IProgressTracker
    {
        private enum ProgressStages
        {
            ResourceEntriesExtract,
            ReportGeneration,
            FetchMapImages
        }

        private readonly IProgress<int> _progress;
        private readonly Dictionary<ProgressStages, int> _progressStageWeights;

        private bool _resourceEntriesProcessed;
        private bool _reportGenerated;
        private int? _fetchImagesCount;
        private int _fetchImagesProcessed;
        private bool _done;

        public ProgressTracker(Action<int> reportHandler)
        {
            _progress = new Progress<int>(reportHandler);

            _progressStageWeights = new Dictionary<ProgressStages, int> {
                { ProgressStages.ResourceEntriesExtract, 2 },
                { ProgressStages.ReportGeneration, 2 },
                { ProgressStages.FetchMapImages, 95 }
            };
        }

        public void ReportResourceEntriesProcessed()
        {
            _resourceEntriesProcessed = true;
            _progress.Report(CalculateValue());
        }

        public void ReportContentGenerationDone()
        {
            _reportGenerated = true;
            _progress.Report(CalculateValue());
        }

        public void ReportDone()
        {
            _done = true;
            _progress.Report(CalculateValue());
        }

        public void ReportFetchImagesCount(int count)
        {
            _fetchImagesCount = count;
            _progress.Report(CalculateValue());
        }

        public void ReportFetchImageProcessed()
        {
            _fetchImagesProcessed++;
            _progress.Report(CalculateValue());
        }

        private int CalculateValue()
        {
            if (_done)
            {
                return 100;
            }

            var sum = 0;

            if (_resourceEntriesProcessed)
            {
                sum += _progressStageWeights[ProgressStages.ResourceEntriesExtract];
            }

            if (_reportGenerated)
            {
                sum += _progressStageWeights[ProgressStages.ReportGeneration];
            }

            if (_fetchImagesCount.HasValue)
            {
                sum += (int) ((float) _fetchImagesProcessed / _fetchImagesCount.Value * _progressStageWeights[ProgressStages.FetchMapImages]);
            }

            return sum;
        }
    }
}
