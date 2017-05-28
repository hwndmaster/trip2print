using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using TripToPrint.Core.ProgressTracking;

namespace TripToPrint.Core.Tests.UnitTests
{
    [TestClass]
    public class ProgressTrackerTests
    {
        private readonly Mock<IProgress<int>> _progressMock = new Mock<IProgress<int>>();

        private ResourceFetchingProgress _tracker;

        [TestInitialize]
        public void TestInitialize()
        {
            _tracker = new ResourceFetchingProgress(_progressMock.Object);
        }

        [TestMethod]
        public void When_resource_entries_process_is_reported_the_progress_is_calculated_properly()
        {
            var value = _tracker.GetProgressStageWeights()[ResourceFetchingProgressStages.ResourceEntriesExtract];

            _tracker.ReportResourceEntriesProcessed();

            _progressMock.Verify(x => x.Report(value));
        }

        [TestMethod]
        public void When_images_fetching_is_reported_the_progress_is_calculated_properly()
        {
            var value = _tracker.GetProgressStageWeights()[ResourceFetchingProgressStages.FetchMapImages];
            var count = 4;

            _tracker.ReportFetchImagesCount(count);
            _progressMock.Verify(x => x.Report(0));

            _tracker.ReportFetchImageProcessed();
            _progressMock.Verify(x => x.Report(value / count), Times.Once);

            _tracker.ReportFetchImageProcessed();
            _progressMock.Verify(x => x.Report(value / count * 2), Times.Once);

            _tracker.ReportFetchImageProcessed();
            _progressMock.Verify(x => x.Report(value / count * 3), Times.Once);

            _tracker.ReportFetchImageProcessed();
            _progressMock.Verify(x => x.Report(value), Times.Once);
        }

        [TestMethod]
        public void When_report_done_is_reported_the_progress_is_calculated_properly()
        {
            _tracker.ReportDone();

            _progressMock.Verify(x => x.Report(100));
        }

        [TestMethod]
        public void When_step_2_is_reported_the_progress_is_calculated_properly()
        {
            var value1 = _tracker.GetProgressStageWeights()[ResourceFetchingProgressStages.ResourceEntriesExtract];
            var value2 = _tracker.GetProgressStageWeights()[ResourceFetchingProgressStages.FetchMapImages];

            _tracker.ReportResourceEntriesProcessed();
            _progressMock.Verify(x => x.Report(value1), Times.Once);

            _tracker.ReportFetchImagesCount(0);
            _progressMock.Verify(x => x.Report(value1 + value2), Times.Once);
        }
    }
}
