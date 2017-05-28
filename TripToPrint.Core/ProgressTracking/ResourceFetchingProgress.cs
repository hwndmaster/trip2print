using System;
using System.Collections.Generic;

namespace TripToPrint.Core.ProgressTracking
{
    public interface IResourceFetchingProgress
    {
        void ReportResourceEntriesProcessed();
        void ReportFetchImagesCount(int count);
        void ReportFetchImageProcessed();
        void ReportDone();
    }

    internal enum ResourceFetchingProgressStages
    {
        ResourceEntriesExtract,
        FetchMapImages,
    }

    public class ResourceFetchingProgress : IResourceFetchingProgress
    {
        private readonly IProgress<int> _progress;
        private readonly Dictionary<ResourceFetchingProgressStages, int> _progressStageWeights;

        private bool _resourceEntriesProcessed;
        private int? _fetchImagesCount;
        private int _fetchImagesProcessed;
        private bool _done;

        public ResourceFetchingProgress(IProgress<int> progress)
        {
            _progress = progress;

            _progressStageWeights = new Dictionary<ResourceFetchingProgressStages, int> {
                { ResourceFetchingProgressStages.ResourceEntriesExtract, 6 },
                { ResourceFetchingProgressStages.FetchMapImages, 93 }
            };
        }

        public void ReportResourceEntriesProcessed()
        {
            _resourceEntriesProcessed = true;
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

        internal Dictionary<ResourceFetchingProgressStages, int> GetProgressStageWeights()
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
                sum += _progressStageWeights[ResourceFetchingProgressStages.ResourceEntriesExtract];
            }

            if (_fetchImagesCount.HasValue)
            {
                if (_fetchImagesCount == 0)
                {
                    sum += _progressStageWeights[ResourceFetchingProgressStages.FetchMapImages];
                }
                else
                {
                    sum += (int)((float)_fetchImagesProcessed / _fetchImagesCount.Value * _progressStageWeights[ResourceFetchingProgressStages.FetchMapImages]);
                }
            }

            return sum;
        }
    }
}
