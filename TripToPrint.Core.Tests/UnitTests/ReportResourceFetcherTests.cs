using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TripToPrint.Core.Logging;
using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;
using TripToPrint.Core.ProgressTracking;

namespace TripToPrint.Core.Tests.UnitTests
{
    [TestClass]
    public class ReportResourceFetcherTests
    {
        private readonly Mock<IMooiDocumentFactory> _mooiDocumentFactoryMock = new Mock<IMooiDocumentFactory>();
        private readonly Mock<IHereAdapter> _hereMock = new Mock<IHereAdapter>();
        private readonly Mock<IFoursquareAdapter> _foursquareMock = new Mock<IFoursquareAdapter>();
        private readonly Mock<IResourceFetchingLogger> _loggerMock = new Mock<IResourceFetchingLogger>();
        private readonly Mock<IFileService> _fileMock = new Mock<IFileService>();
        private readonly Mock<IResourceNameProvider> _resourceNameMock = new Mock<IResourceNameProvider>();
        private readonly Mock<IResourceFetchingProgress> _progressMock = new Mock<IResourceFetchingProgress>();

        private Mock<ReportResourceFetcher> _fetcher;

        private string _tempFolderName = "temp-folder-name";

        [TestInitialize]
        public void TestInitialize()
        {
            _fetcher = new Mock<ReportResourceFetcher>(
                _mooiDocumentFactoryMock.Object,
                _hereMock.Object,
                _foursquareMock.Object,
                _loggerMock.Object,
                _fileMock.Object,
                _resourceNameMock.Object) {
                CallBase = true
            };

            _resourceNameMock.Setup(x => x.GetDefaultHtmlReportName()).Returns("default");
            _resourceNameMock.Setup(x => x.CreateTempFolderName(It.IsAny<string>())).Returns(_tempFolderName);

            _resourceNameMock.Setup(x => x.CreateFileNameForOverviewMap(It.IsAny<MooiGroup>())).Returns(string.Empty);
            _resourceNameMock.Setup(x => x.CreateFileNameForPlacemarkThumbnail(It.IsAny<MooiPlacemark>())).Returns(string.Empty);
        }

        [TestMethod]
        public async Task When_generating_report_the_report_is_created_and_path_is_returned()
        {
            // Arrange
            var document = new KmlDocument {
                Resources = {
                    new KmlResource { FileName = "resource-1", Blob = new byte[] {1}},
                    new KmlResource { FileName = "resource-2", Blob = new byte[] {2}}
                }
            };
            _fetcher.Setup(x => x.FetchMapImages(It.IsAny<MooiDocument>(), It.IsAny<string>(), _progressMock.Object))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _fetcher.Object.Generate(document, new List<DiscoveredPlace>(), null, _progressMock.Object);

            // Verify
            Assert.IsTrue(result.tempPath.StartsWith(Path.Combine(Path.GetTempPath(), _tempFolderName)));
            AssertDocumentResourcesAreSaved(document);
            _fetcher.Verify(x => x.FetchMapImages(It.IsAny<MooiDocument>(), result.tempPath, _progressMock.Object), Times.Once);
        }

        [TestMethod]
        public async Task When_generating_report_for_kmz_the_progress_is_being_updated_properly()
        {
            // Arrange
            var document = new KmlDocument();
            _fetcher.Setup(x => x.FetchMapImages(It.IsAny<MooiDocument>(), It.IsAny<string>(), _progressMock.Object))
                .Returns(Task.CompletedTask);

            // Act
            await _fetcher.Object.Generate(document, new List<DiscoveredPlace>(), null, _progressMock.Object);

            // Verify
            _progressMock.Verify(x => x.ReportResourceEntriesProcessed(), Times.Once);
            _progressMock.Verify(x => x.ReportDone(), Times.Once);
        }

        [TestMethod]
        public async Task When_fetching_images_the_progress_is_being_updated_and_images_downloaded()
        {
            // Arrange
            var placemark = new MooiPlacemark();
            var group = new MooiGroup { Placemarks = { placemark } };
            var document = new MooiDocument { Sections = { new MooiSection { Groups = { group } } } };
            var tempPath = "temp-path";
            var overviewBytes = new byte[] { 0x22 };
            var thumbnailBytes = new byte[] { 0x44 };
            _resourceNameMock.Setup(x => x.CreateFileNameForOverviewMap(group)).Returns("overview-path");
            _resourceNameMock.Setup(x => x.CreateFileNameForPlacemarkThumbnail(placemark)).Returns("thumb-path");
            _hereMock.Setup(x => x.FetchOverviewMap(group))
                .Returns(Task.FromResult(overviewBytes));
            _hereMock.Setup(x => x.FetchThumbnail(placemark))
                .Returns(Task.FromResult(thumbnailBytes));

            // Act
            await _fetcher.Object.FetchMapImages(document, tempPath, _progressMock.Object);

            // Verify
            _fileMock.Verify(x => x.WriteBytesAsync(tempPath + @"\overview-path", overviewBytes));
            _fileMock.Verify(x => x.WriteBytesAsync(tempPath + @"\thumb-path", thumbnailBytes));
            _progressMock.Verify(x => x.ReportFetchImagesCount(1 + 1), Times.Once);
            _progressMock.Verify(x => x.ReportFetchImageProcessed(), Times.Exactly(2));
        }

        private void AssertDocumentResourcesAreSaved(KmlDocument document)
        {
            foreach (var entry in document.Resources)
            {
                _fileMock.Verify(x => x.WriteBytesAsync(It.Is<string>(s => s.Contains(entry.FileName)), entry.Blob));
            }
        }
    }
}
