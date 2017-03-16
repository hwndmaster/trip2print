using System;
using System.Collections.Generic;

namespace TripToPrint.Core
{
    public interface IProgressTracker
    {
        void ReportResourceEntriesProcessed();
        void ReportContentGenerationDone();
        void ReportFetchImagesCount(int count);
        void ReportFetchImageProcessed();
        void ReportDone();
    }

    internal enum ProgressStages
    {
        ResourceEntriesExtract,
        ReportGeneration,
        FetchMapImages,
    }

    public class ProgressTracker : IProgressTracker
    {
        private readonly IProgress<int> _progress;
        private readonly Dictionary<ProgressStages, int> _progressStageWeights;

        private bool _resourceEntriesProcessed;
        private bool _reportGenerated;
        private int? _fetchImagesCount;
        private int _fetchImagesProcessed;
        private bool _done;

        public ProgressTracker(IProgress<int> progress)
        {
            _progress = progress;

            _progressStageWeights = new Dictionary<ProgressStages, int> {
                { ProgressStages.ResourceEntriesExtract, 3 },
                { ProgressStages.ReportGeneration, 3 },
                { ProgressStages.FetchMapImages, 93 }
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

        public void ReportDone()
        {
            _done = true;
            _progress.Report(CalculateValue());
        }

        internal Dictionary<ProgressStages, int> GetProgressStageWeights()
        {
            return _progressStageWeights;
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
                if (_fetchImagesCount == 0)
                {
                    sum += _progressStageWeights[ProgressStages.FetchMapImages];
                }
                else
                {
                    sum += (int)((float)_fetchImagesProcessed / _fetchImagesCount.Value * _progressStageWeights[ProgressStages.FetchMapImages]);
                }
            }

            return sum;
        }
    }
}
